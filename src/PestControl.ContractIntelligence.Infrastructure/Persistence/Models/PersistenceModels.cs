namespace PestControl.ContractIntelligence.Infrastructure.Persistence.Models;

public sealed class UserRecord
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LockedUntilUtc { get; set; }
    public ICollection<RefreshTokenRecord> RefreshTokens { get; set; } = [];
}

public sealed class RefreshTokenRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public UserRecord User { get; set; } = null!;
}

public sealed class DocumentRecord
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public string ContentSha256 { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public int Status { get; set; }
    public int? PageCount { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? LastIndexedAtUtc { get; set; }
    public string? FailureCode { get; set; }
    public ICollection<DocumentPageRecord> Pages { get; set; } = [];
    public ICollection<DocumentChunkRecord> Chunks { get; set; } = [];
    public ICollection<IngestionJobRecord> IngestionJobs { get; set; } = [];
}

public sealed class DocumentPageRecord
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public string Text { get; set; } = null!;
    public string ExtractionMethod { get; set; } = null!;
    public DocumentRecord Document { get; set; } = null!;
}

public sealed class DocumentChunkRecord
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = null!;
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int? TokenCount { get; set; }
    public string? EmbeddingModel { get; set; }
    public int? EmbeddingDimension { get; set; }
    public byte[]? Embedding { get; set; }
    public DocumentRecord Document { get; set; } = null!;
    public ICollection<MessageSourceRecord> MessageSources { get; set; } = [];
}

public sealed class IngestionJobRecord
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? FailureCode { get; set; }
    public DocumentRecord Document { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class AiProviderConfigurationRecord
{
    public Guid Id { get; set; }
    public int ProviderType { get; set; }
    public string ChatModel { get; set; } = null!;
    public string EmbeddingModel { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class ChatSessionRecord
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ArchivedAtUtc { get; set; }
    public ICollection<ChatMessageRecord> Messages { get; set; } = [];
}

public sealed class ChatMessageRecord
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int Role { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ChatSessionRecord Session { get; set; } = null!;
    public ICollection<MessageSourceRecord> Sources { get; set; } = [];
}

public sealed class MessageSourceRecord
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid ChunkId { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public string Excerpt { get; set; } = null!;
    public ChatMessageRecord Message { get; set; } = null!;
    public DocumentChunkRecord Chunk { get; set; } = null!;
}

public sealed class AuditLogRecord
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = null!;
    public string ResourceType { get; set; } = null!;
    public string? ResourceId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
