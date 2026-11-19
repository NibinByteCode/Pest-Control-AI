namespace PestControl.ContractIntelligence.Application.Abstractions.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
