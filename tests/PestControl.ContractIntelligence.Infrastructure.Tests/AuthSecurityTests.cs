using PestControl.ContractIntelligence.Infrastructure.Security;

namespace PestControl.ContractIntelligence.Infrastructure.Tests;

public sealed class AuthSecurityTests
{
    [Fact]
    public void PasswordHasherVerifiesCorrectPasswordAndRejectsWrongPassword()
    {
        var hasher = new SystemPasswordHasher();

        var hash = hasher.Hash("Correct Horse Battery");

        Assert.True(hasher.Verify("Correct Horse Battery", hash));
        Assert.False(hasher.Verify("incorrect password", hash));
        Assert.NotEqual("Correct Horse Battery", hash);
    }

    [Fact]
    public void RefreshTokenHashIsDeterministicAndNotPlainText()
    {
        const string token = "random-refresh-token";

        var first = JwtTokenService.RefreshTokenHash(token);
        var second = JwtTokenService.RefreshTokenHash(token);

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
        Assert.DoesNotContain(token, first);
    }
}
