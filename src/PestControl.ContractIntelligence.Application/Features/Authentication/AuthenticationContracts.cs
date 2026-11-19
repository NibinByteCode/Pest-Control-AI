using PestControl.ContractIntelligence.Application.Common.Validation;
using PestControl.ContractIntelligence.Domain.Identity;

namespace PestControl.ContractIntelligence.Application.Features.Authentication;

public sealed record BootstrapAdminRequest(string UserName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record AuthenticationResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);

public interface IAuthenticationService
{
    Task<AuthenticationResponse> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken cancellationToken);
    Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthenticationResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
}

public static class AuthenticationRequestValidator
{
    public static ValidationResult Validate(BootstrapAdminRequest request)
    {
        var result = RequestValidation.ValidateEmail(request.Email);
        result.AddIf(string.IsNullOrWhiteSpace(request.UserName), "User name is required.");
        result.AddIf(!string.IsNullOrWhiteSpace(request.UserName) && request.UserName.Trim().Length < 3, "User name must be at least 3 characters.");
        foreach (var error in RequestValidation.ValidatePassword(request.Password).Errors) result.Add(error);
        return result;
    }

    public static ValidationResult Validate(LoginRequest request)
    {
        var result = RequestValidation.ValidateEmail(request.Email);
        result.AddIf(string.IsNullOrWhiteSpace(request.Password), "Password is required.");
        return result;
    }

    public static ValidationResult Validate(RefreshRequest request)
        => RequestValidation.ValidateRequired(request.RefreshToken, "Refresh token");
}
