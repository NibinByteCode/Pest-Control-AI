namespace PestControl.ContractIntelligence.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
