namespace PestControl.ContractIntelligence.Api.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public string ApplicationDb { get; set; } = string.Empty;
}
