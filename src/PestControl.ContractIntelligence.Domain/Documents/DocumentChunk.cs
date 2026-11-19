using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Documents;

public sealed class DocumentChunk : Entity
{
    private DocumentChunk(Guid id, Guid documentId, int chunkIndex, string text, int startPage, int endPage)
        : base(id)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document ID must not be empty.", nameof(documentId));
        ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startPage);
        ArgumentOutOfRangeException.ThrowIfLessThan(endPage, startPage);

        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        Text = text.Trim();
        StartPage = startPage;
        EndPage = endPage;
    }

    public Guid DocumentId { get; }
    public int ChunkIndex { get; }
    public string Text { get; }
    public int StartPage { get; }
    public int EndPage { get; }
    public int? TokenCount { get; private set; }
    public string? EmbeddingModel { get; private set; }
    public int? EmbeddingDimension { get; private set; }

    public static DocumentChunk Create(Guid documentId, int chunkIndex, string text, int startPage, int endPage)
        => new(Guid.NewGuid(), documentId, chunkIndex, text, startPage, endPage);

    public void SetEmbeddingMetadata(string model, int dimension, int tokenCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimension);
        ArgumentOutOfRangeException.ThrowIfNegative(tokenCount);

        EmbeddingModel = model.Trim();
        EmbeddingDimension = dimension;
        TokenCount = tokenCount;
    }
}
