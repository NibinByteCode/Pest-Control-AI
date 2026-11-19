using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Chat;

public enum MessageRole
{
    User = 1,
    Assistant = 2
}

public sealed class ChatMessage : Entity
{
    private ChatMessage(Guid id, Guid sessionId, MessageRole role, string content)
        : base(id)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Session ID must not be empty.", nameof(sessionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        SessionId = sessionId;
        Role = role;
        Content = content.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid SessionId { get; }
    public MessageRole Role { get; }
    public string Content { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static ChatMessage Create(Guid sessionId, MessageRole role, string content)
        => new(Guid.NewGuid(), sessionId, role, content);
}
