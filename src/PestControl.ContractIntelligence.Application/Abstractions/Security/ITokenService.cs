namespace PestControl.ContractIntelligence.Application.Abstractions.Security;

public interface ITokenService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<RefreshTokenResult> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken);
}

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAtUtc);
public sealed record RefreshTokenResult(string Token, DateTimeOffset ExpiresAtUtc, string TokenHash);
public sealed record RefreshTokenValidationResult(bool IsValid, Guid? UserId, string? TokenHash);
