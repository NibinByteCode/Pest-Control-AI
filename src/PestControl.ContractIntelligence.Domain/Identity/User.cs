using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Identity;

public sealed class User : Entity
{
    private User(Guid id, string userName, string normalizedEmail, string passwordHash, Role role)
        : base(id)
    {
        UserName = NormalizeRequired(userName, nameof(userName));
        NormalizedEmail = NormalizeRequired(normalizedEmail, nameof(normalizedEmail));
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public string UserName { get; }
    public string NormalizedEmail { get; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public static User Create(string userName, string email, string passwordHash, Role role)
    {
        return new User(Guid.NewGuid(), userName, email.Trim().ToUpperInvariant(), passwordHash, role);
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
        LockedUntilUtc = null;
    }

    public void ChangeRole(Role role) => Role = role;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void LockUntil(DateTimeOffset untilUtc)
    {
        if (untilUtc <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(untilUtc), "Lock time must be in the future.");
        }

        LockedUntilUtc = untilUtc;
    }

    public bool IsLocked(DateTimeOffset nowUtc) => LockedUntilUtc is { } lockedUntil && lockedUntil > nowUtc;

    private static string NormalizeRequired(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
