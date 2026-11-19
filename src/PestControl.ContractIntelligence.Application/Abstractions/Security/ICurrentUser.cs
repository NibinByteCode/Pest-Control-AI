using PestControl.ContractIntelligence.Domain.Identity;

namespace PestControl.ContractIntelligence.Application.Abstractions.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Role? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(Role role);
}
