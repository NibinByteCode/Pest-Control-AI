namespace PestControl.ContractIntelligence.Application.Common.Validation;

public sealed class ValidationResult
{
    private readonly List<string> errors = [];

    public bool IsValid => errors.Count == 0;
    public IReadOnlyList<string> Errors => errors;

    public void Add(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        errors.Add(message);
    }

    public void AddIf(bool condition, string message)
    {
        if (condition) Add(message);
    }
}
