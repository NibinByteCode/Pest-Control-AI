using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Documents;

public sealed class Document : Entity
{
    private Document(Guid id, string originalFileName, string storageKey, string contentSha256, long fileSizeBytes)
        : base(id)
    {
        OriginalFileName = Required(originalFileName, nameof(originalFileName));
        StorageKey = Required(storageKey, nameof(storageKey));
        ContentSha256 = Required(contentSha256, nameof(contentSha256));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeBytes);

        FileSizeBytes = fileSizeBytes;
        Status = DocumentStatus.Discovered;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public string OriginalFileName { get; }
    public string StorageKey { get; }
    public string ContentSha256 { get; private set; }
    public long FileSizeBytes { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int? PageCount { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? LastIndexedAtUtc { get; private set; }
    public string? FailureCode { get; private set; }

    public static Document Discover(string originalFileName, string storageKey, string contentSha256, long fileSizeBytes)
    {
        return new Document(Guid.NewGuid(), originalFileName, storageKey, contentSha256, fileSizeBytes);
    }

    public void Queue()
    {
        RequireStatus(DocumentStatus.Discovered, DocumentStatus.Failed);
        TransitionTo(DocumentStatus.Queued);
        FailureCode = null;
    }

    public void BeginExtraction(int pageCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageCount);

        RequireStatus(DocumentStatus.Queued);
        PageCount = pageCount;
        TransitionTo(DocumentStatus.Extracting);
    }

    public void BeginOcr() { RequireStatus(DocumentStatus.Extracting); TransitionTo(DocumentStatus.Ocr); }
    public void BeginChunking() { RequireStatus(DocumentStatus.Extracting, DocumentStatus.Ocr); TransitionTo(DocumentStatus.Chunking); }
    public void BeginEmbedding() { RequireStatus(DocumentStatus.Chunking); TransitionTo(DocumentStatus.Embedding); }

    public void MarkIndexed()
    {
        RequireStatus(DocumentStatus.Embedding);
        TransitionTo(DocumentStatus.Indexed);
        LastIndexedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Fail(string failureCode)
    {
        FailureCode = Required(failureCode, nameof(failureCode));
        TransitionTo(DocumentStatus.Failed);
    }

    public void CreateNewVersion(string contentSha256, long fileSizeBytes)
    {
        Required(contentSha256, nameof(contentSha256));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeBytes);

        ContentSha256 = contentSha256.Trim();
        FileSizeBytes = fileSizeBytes;
        Version++;
        FailureCode = null;
        LastIndexedAtUtc = null;
        TransitionTo(DocumentStatus.Discovered);
    }

    private void RequireStatus(params DocumentStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException($"Document cannot transition from {Status}.");
        }
    }

    private void TransitionTo(DocumentStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
