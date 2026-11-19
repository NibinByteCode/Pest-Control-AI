using PestControl.ContractIntelligence.Application.Common.Validation;

namespace PestControl.ContractIntelligence.Application.Features.Documents;

public sealed record DocumentRootSummary(Guid Id, string Name, string RootPath, bool IsActive);
public sealed record RegisterDocumentRootRequest(string Name, string RootPath);
public sealed record DocumentSummary(Guid Id, string FileName, string StorageKey, string Status, int Version, DateTimeOffset? LastIndexedAtUtc);
public sealed record IngestionJobSummary(Guid Id, Guid DocumentId, string Status, int AttemptCount, string? FailureCode);

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentRootSummary>> ListRootsAsync(CancellationToken cancellationToken);
    Task<DocumentRootSummary> RegisterRootAsync(RegisterDocumentRootRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentSummary>> ListDocumentsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<IngestionJobSummary>> ListJobsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Guid> RequestScanAsync(Guid rootId, CancellationToken cancellationToken);
    Task RetryJobAsync(Guid jobId, CancellationToken cancellationToken);
}

public static class DocumentRequestValidator
{
    public static ValidationResult Validate(RegisterDocumentRootRequest request)
    {
        var result = new ValidationResult();
        result.AddIf(string.IsNullOrWhiteSpace(request.Name), "Root name is required.");
        result.AddIf(string.IsNullOrWhiteSpace(request.RootPath), "Root path is required.");
        result.AddIf(!string.IsNullOrWhiteSpace(request.RootPath) &&
            request.RootPath.Contains("..", StringComparison.Ordinal), "Root path must not contain traversal segments.");
        return result;
    }

    public static ValidationResult ValidatePaging(int page, int pageSize)
    {
        var result = new ValidationResult();
        result.AddIf(page < 1, "Page must be at least 1.");
        result.AddIf(pageSize is < 1 or > 100, "Page size must be between 1 and 100.");
        return result;
    }
}
