namespace PestControl.ContractIntelligence.Domain.Documents;

public enum DocumentStatus
{
    Discovered = 1,
    Queued = 2,
    Extracting = 3,
    Ocr = 4,
    Chunking = 5,
    Embedding = 6,
    Indexed = 7,
    Failed = 8
}
