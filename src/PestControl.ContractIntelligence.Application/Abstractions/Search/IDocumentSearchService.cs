namespace PestControl.ContractIntelligence.Application.Abstractions.Search;

public interface IDocumentSearchService
{
    Task<IReadOnlyList<SearchEvidence>> SearchAsync(
        string query,
        Guid userId,
        int maximumResults,
        CancellationToken cancellationToken);
}

public sealed record SearchEvidence(
    Guid ChunkId,
    Guid DocumentId,
    string DocumentName,
    int StartPage,
    int EndPage,
    string Excerpt,
    double Score);
