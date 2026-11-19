using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Chat;

public sealed class MessageSource : Entity
{
    private MessageSource(Guid id, Guid messageId, Guid chunkId, int startPage, int endPage, string excerpt)
        : base(id)
    {
        if (messageId == Guid.Empty) throw new ArgumentException("Message ID must not be empty.", nameof(messageId));
        if (chunkId == Guid.Empty) throw new ArgumentException("Chunk ID must not be empty.", nameof(chunkId));
        if (startPage <= 0 || endPage < startPage) throw new ArgumentOutOfRangeException(nameof(startPage));
        ArgumentException.ThrowIfNullOrWhiteSpace(excerpt);

        MessageId = messageId;
        ChunkId = chunkId;
        StartPage = startPage;
        EndPage = endPage;
        Excerpt = excerpt.Trim();
    }

    public Guid MessageId { get; }
    public Guid ChunkId { get; }
    public int StartPage { get; }
    public int EndPage { get; }
    public string Excerpt { get; }

    public static MessageSource Create(Guid messageId, Guid chunkId, int startPage, int endPage, string excerpt)
        => new(Guid.NewGuid(), messageId, chunkId, startPage, endPage, excerpt);
}
