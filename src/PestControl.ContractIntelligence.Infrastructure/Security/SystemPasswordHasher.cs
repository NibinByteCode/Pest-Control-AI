using Microsoft.AspNetCore.Identity;
using PestControl.ContractIntelligence.Application.Abstractions.Security;

namespace PestControl.ContractIntelligence.Infrastructure.Security;

public sealed class SystemPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> hasher = new();

    public string Hash(string password)
        => hasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash)
    {
        var result = hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
