using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Chat;

public sealed class ChatSession : Entity
{
    private ChatSession(Guid id, Guid ownerId, string title)
        : base(id)
    {
        if (ownerId == Guid.Empty) throw new ArgumentException("Owner ID must not be empty.", nameof(ownerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        OwnerId = ownerId;
        Title = title.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid OwnerId { get; }
    public string Title { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static ChatSession Create(Guid ownerId, string title)
        => new(Guid.NewGuid(), ownerId, title);

    public void Rename(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
    }

    public void Archive()
    {
        ArchivedAtUtc ??= DateTimeOffset.UtcNow;
    }
}
