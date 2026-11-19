using PestControl.ContractIntelligence.Application.Common.Validation;
using PestControl.ContractIntelligence.Domain.Identity;

namespace PestControl.ContractIntelligence.Application.Features.Users;

public sealed record UserSummary(Guid Id, string UserName, string Email, Role Role, bool IsActive);
public sealed record CreateUserRequest(string UserName, string Email, string Password, Role Role);
public sealed record UpdateUserRequest(string? Password, Role? Role, bool? IsActive);

public interface IUserService
{
    Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken);
    Task<UserSummary> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserSummary> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken);
}

public static class UserRequestValidator
{
    public static ValidationResult Validate(CreateUserRequest request)
    {
        var result = RequestValidation.ValidateEmail(request.Email);
        result.AddIf(string.IsNullOrWhiteSpace(request.UserName), "User name is required.");
        foreach (var error in RequestValidation.ValidatePassword(request.Password).Errors) result.Add(error);
        return result;
    }
}
