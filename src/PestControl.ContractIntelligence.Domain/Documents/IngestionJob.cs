using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Documents;

public sealed class IngestionJob : Entity
{
    private IngestionJob(Guid id, Guid documentId)
        : base(id)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document ID must not be empty.", nameof(documentId));
        DocumentId = documentId;
        Status = IngestionJobStatus.Queued;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid DocumentId { get; }
    public IngestionJobStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? FailureCode { get; private set; }

    public static IngestionJob Queue(Guid documentId) => new(Guid.NewGuid(), documentId);

    public void Start()
    {
        if (Status != IngestionJobStatus.Queued && Status != IngestionJobStatus.Failed)
            throw new InvalidOperationException($"Job cannot start from {Status}.");
        Status = IngestionJobStatus.Running;
        AttemptCount++;
        StartedAtUtc = DateTimeOffset.UtcNow;
        FailureCode = null;
    }

    public void Succeed()
    {
        RequireRunning();
        Status = IngestionJobStatus.Succeeded;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Fail(string failureCode)
    {
        RequireRunning();
        ArgumentException.ThrowIfNullOrWhiteSpace(failureCode);
        FailureCode = failureCode.Trim();
        Status = IngestionJobStatus.Failed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is IngestionJobStatus.Succeeded or IngestionJobStatus.Cancelled)
            throw new InvalidOperationException($"Job cannot be cancelled from {Status}.");
        Status = IngestionJobStatus.Cancelled;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    private void RequireRunning()
    {
        if (Status != IngestionJobStatus.Running)
            throw new InvalidOperationException($"Job must be running, but is {Status}.");
    }
}
