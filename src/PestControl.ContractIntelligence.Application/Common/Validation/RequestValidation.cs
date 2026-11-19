namespace PestControl.ContractIntelligence.Application.Common.Validation;

public static class RequestValidation
{
    public static ValidationResult ValidateRequired(string value, string fieldName)
    {
        var result = new ValidationResult();
        result.AddIf(string.IsNullOrWhiteSpace(value), $"{fieldName} is required.");
        return result;
    }

    public static ValidationResult ValidateEmail(string email)
    {
        var result = ValidateRequired(email, "Email");
        result.AddIf(!string.IsNullOrWhiteSpace(email) &&
            !email.Contains('@', StringComparison.Ordinal), "Email must be valid.");
        return result;
    }

    public static ValidationResult ValidatePassword(string password)
    {
        var result = ValidateRequired(password, "Password");
        result.AddIf(password is { Length: > 0 and < 12 }, "Password must be at least 12 characters.");
        return result;
    }
}
