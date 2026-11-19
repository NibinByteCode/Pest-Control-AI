namespace PestControl.ContractIntelligence.Domain.Documents;

public enum IngestionJobStatus
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}
