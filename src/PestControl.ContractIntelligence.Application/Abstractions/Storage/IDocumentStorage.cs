namespace PestControl.ContractIntelligence.Application.Abstractions.Storage;

public interface IDocumentStorage
{
    Task<IReadOnlyList<StoredDocument>> EnumeratePdfAsync(Guid rootId, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(Guid rootId, string storageKey, CancellationToken cancellationToken);
    Task<bool> RootIsAccessibleAsync(Guid rootId, CancellationToken cancellationToken);
}

public sealed record StoredDocument(string FileName, string StorageKey, long Length, string Sha256);
