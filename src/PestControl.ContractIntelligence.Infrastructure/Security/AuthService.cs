using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PestControl.ContractIntelligence.Application.Abstractions.Security;
using PestControl.ContractIntelligence.Application.Common.Exceptions;
using PestControl.ContractIntelligence.Application.Common.Validation;
using PestControl.ContractIntelligence.Application.Features.Authentication;
using PestControl.ContractIntelligence.Domain.Identity;
using PestControl.ContractIntelligence.Infrastructure.Persistence;
using PestControl.ContractIntelligence.Infrastructure.Persistence.Models;

namespace PestControl.ContractIntelligence.Infrastructure.Security;

public sealed class AuthService(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthenticationService
{
    private static readonly ConcurrentDictionary<string, FailureState> Failures = new(StringComparer.OrdinalIgnoreCase);

    public async Task<AuthenticationResponse> BootstrapAdminAsync(
        BootstrapAdminRequest request,
        CancellationToken cancellationToken)
    {
        var validation = AuthenticationRequestValidator.Validate(request);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        if (await dbContext.Users.AnyAsync(x => x.Role == (int)Role.Admin, cancellationToken))
        {
            throw new InvalidOperationException("Administrator bootstrap is disabled.");
        }

        var record = CreateUser(request.UserName, request.Email, request.Password, Role.Admin);
        dbContext.Users.Add(record);
        dbContext.AuditLogs.Add(CreateAudit(null, "auth.bootstrap", "User", record.Id.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(record.Id, cancellationToken);
    }

    public async Task<AuthenticationResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validation = AuthenticationRequestValidator.Validate(request);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var email = request.Email.Trim().ToUpperInvariant();
        var stateKey = email;
        if (Failures.TryGetValue(stateKey, out var state) &&
            state.LockedUntilUtc is { } lockedUntil &&
            lockedUntil > DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("The account is temporarily locked.");
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.NormalizedEmail == email, cancellationToken);

        if (user is null || !user.IsActive ||
            !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            RegisterFailure(stateKey);
            dbContext.AuditLogs.Add(CreateAudit(user?.Id, "auth.login.failed", "User", user?.Id.ToString()));
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        Failures.TryRemove(stateKey, out _);
        dbContext.AuditLogs.Add(CreateAudit(user.Id, "auth.login.success", "User", user.Id.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user.Id, cancellationToken);
    }

    public async Task<AuthenticationResponse> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var validation = AuthenticationRequestValidator.Validate(request);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var result = await tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (!result.IsValid || result.UserId is not { } userId)
        {
            if (request.RefreshToken is not null)
            {
                var hash = JwtTokenService.RefreshTokenHash(request.RefreshToken);
                var reused = await dbContext.RefreshTokens
                    .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
                if (reused?.RevokedAtUtc is not null)
                {
                    var activeTokens = await dbContext.RefreshTokens
                        .Where(x => x.UserId == reused.UserId && x.RevokedAtUtc == null)
                        .ToListAsync(cancellationToken);
                    foreach (var token in activeTokens)
                    {
                        token.RevokedAtUtc = DateTimeOffset.UtcNow;
                    }
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            throw new UnauthorizedAccessException("Refresh token is invalid or revoked.");
        }

        var oldRecord = await dbContext.RefreshTokens
            .SingleAsync(x => x.TokenHash == result.TokenHash, cancellationToken);
        oldRecord.RevokedAtUtc = DateTimeOffset.UtcNow;

        var replacement = await tokenService.CreateRefreshTokenAsync(userId, cancellationToken);
        oldRecord.ReplacedByTokenId = await FindReplacementIdAsync(replacement.TokenHash, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        var access = await tokenService.CreateAccessTokenAsync(userId, cancellationToken);
        return new AuthenticationResponse(
            access.Token,
            access.ExpiresAtUtc,
            replacement.Token,
            replacement.ExpiresAtUtc);
    }

    public async Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var validation = RequestValidation.ValidateRequired(request.RefreshToken, "Refresh token");
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var hash = JwtTokenService.RefreshTokenHash(request.RefreshToken);
        var record = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (record is not null)
        {
            record.RevokedAtUtc = DateTimeOffset.UtcNow;
            dbContext.AuditLogs.Add(CreateAudit(record.UserId, "auth.logout", "User", record.UserId.ToString()));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private UserRecord CreateUser(string userName, string email, string password, Role role)
        => new()
        {
            Id = Guid.NewGuid(),
            UserName = userName.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            PasswordHash = passwordHasher.Hash(password),
            Role = (int)role,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

    private async Task<AuthenticationResponse> IssueTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var access = await tokenService.CreateAccessTokenAsync(userId, cancellationToken);
        var refresh = await tokenService.CreateRefreshTokenAsync(userId, cancellationToken);
        return new AuthenticationResponse(
            access.Token,
            access.ExpiresAtUtc,
            refresh.Token,
            refresh.ExpiresAtUtc);
    }

    private async Task<Guid?> FindReplacementIdAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        return record?.Id;
    }

    private static void RegisterFailure(string key)
    {
        var state = Failures.AddOrUpdate(
            key,
            _ => new FailureState(1, null),
            (_, current) =>
            {
                var count = current.FailureCount + 1;
                var lockedUntil = count >= 5
                    ? DateTimeOffset.UtcNow.AddMinutes(15)
                    : current.LockedUntilUtc;
                return new FailureState(count, lockedUntil);
            });
    }

    private static AuditLogRecord CreateAudit(
        Guid? actorId,
        string action,
        string resourceType,
        string? resourceId)
        => new()
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

    private sealed record FailureState(int FailureCount, DateTimeOffset? LockedUntilUtc);
}
