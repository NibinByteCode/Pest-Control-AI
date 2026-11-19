using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PestControl.ContractIntelligence.Application.Abstractions.Security;
using PestControl.ContractIntelligence.Domain.Identity;
using PestControl.ContractIntelligence.Infrastructure.Persistence;
using PestControl.ContractIntelligence.Infrastructure.Persistence.Models;

namespace PestControl.ContractIntelligence.Infrastructure.Security;

public sealed class JwtTokenService(
    ApplicationDbContext dbContext,
    IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions options = options.Value;

    public Task<AccessTokenResult> CreateAccessTokenAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = dbContext.Users.SingleOrDefault(x => x.Id == userId)
            ?? throw new InvalidOperationException("User was not found.");

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(options.AccessTokenMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.Role, ((Role)user.Role).ToString())
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return Task.FromResult(new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires));
    }

    public async Task<RefreshTokenResult> CreateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToHexString(bytes);
        var hash = RefreshTokenHash(token);
        var expires = DateTimeOffset.UtcNow.AddDays(options.RefreshTokenDays);
        var record = new RefreshTokenRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            ExpiresAtUtc = expires
        };

        dbContext.RefreshTokens.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(token, expires, hash);
    }

    public async Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var hash = RefreshTokenHash(token);
        var record = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (record is null ||
            record.RevokedAtUtc is not null ||
            record.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return new RefreshTokenValidationResult(false, null, null);
        }

        return new RefreshTokenValidationResult(true, record.UserId, hash);
    }

    public static string RefreshTokenHash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
