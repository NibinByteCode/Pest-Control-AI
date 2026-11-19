using PestControl.ContractIntelligence.Domain.Documents;

namespace PestControl.ContractIntelligence.Domain.Tests;

public sealed class DocumentTests
{
    [Fact]
    public void ProcessingLifecycleReachesIndexed()
    {
        var document = Document.Discover("contract.pdf", "root/contract.pdf", "hash", 100);

        document.Queue();
        document.BeginExtraction(3);
        document.BeginChunking();
        document.BeginEmbedding();
        document.MarkIndexed();

        Assert.Equal(DocumentStatus.Indexed, document.Status);
        Assert.Equal(3, document.PageCount);
        Assert.NotNull(document.LastIndexedAtUtc);
    }

    [Fact]
    public void InvalidTransitionIsRejected()
    {
        var document = Document.Discover("contract.pdf", "root/contract.pdf", "hash", 100);

        Assert.Throws<InvalidOperationException>(document.MarkIndexed);
    }

    [Fact]
    public void NewVersionResetsProcessingStateAndUpdatesSourceMetadata()
    {
        var document = Document.Discover("contract.pdf", "root/contract.pdf", "hash", 100);
        document.Queue();
        document.BeginExtraction(1);
        document.BeginChunking();
        document.BeginEmbedding();
        document.MarkIndexed();

        document.CreateNewVersion("new-hash", 200);

        Assert.Equal(2, document.Version);
        Assert.Equal("new-hash", document.ContentSha256);
        Assert.Equal(200, document.FileSizeBytes);
        Assert.Equal(DocumentStatus.Discovered, document.Status);
        Assert.Null(document.LastIndexedAtUtc);
    }
}
