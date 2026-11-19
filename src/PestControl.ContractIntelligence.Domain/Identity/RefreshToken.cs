using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Identity;

public sealed class RefreshToken : Entity
{
    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
        : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must not be empty.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "Token must expire in the future.");
        }

        UserId = userId;
        TokenHash = tokenHash.Trim();
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; }
    public string TokenHash { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc);
    }

    public bool IsUsable(DateTimeOffset nowUtc) => RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Revoke(DateTimeOffset revokedAtUtc, Guid? replacedByTokenId = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(revokedAtUtc, DateTimeOffset.UtcNow.AddMinutes(1));

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
    }
}
