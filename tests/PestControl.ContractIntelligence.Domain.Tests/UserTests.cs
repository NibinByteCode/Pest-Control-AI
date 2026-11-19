using PestControl.ContractIntelligence.Domain.Identity;

namespace PestControl.ContractIntelligence.Domain.Tests;

public sealed class UserTests
{
    [Fact]
    public void CreateNormalizesEmail()
    {
        var user = User.Create(" manager ", " Manager@Example.com ", "hash", Role.Manager);

        Assert.Equal("manager", user.UserName);
        Assert.Equal("MANAGER@EXAMPLE.COM", user.NormalizedEmail);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void LockUntilRejectsPastTime()
    {
        var user = User.Create("manager", "manager@example.com", "hash", Role.Manager);

        Assert.Throws<ArgumentOutOfRangeException>(() => user.LockUntil(DateTimeOffset.UtcNow.AddMinutes(-1)));
    }
}
