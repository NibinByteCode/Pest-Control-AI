using PestControl.ContractIntelligence.Domain.Documents;

namespace PestControl.ContractIntelligence.Domain.Tests;

public sealed class DocumentChunkTests
{
    [Fact]
    public void InvalidPageRangeIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DocumentChunk.Create(Guid.NewGuid(), 0, "clause", 4, 2));
    }

    [Fact]
    public void EmbeddingMetadataRequiresPositiveDimension()
    {
        var chunk = DocumentChunk.Create(Guid.NewGuid(), 0, "clause", 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            chunk.SetEmbeddingMetadata("model", 0, 3));
    }
}
