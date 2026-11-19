using Microsoft.EntityFrameworkCore;
using PestControl.ContractIntelligence.Application.Abstractions.Persistence;
using PestControl.ContractIntelligence.Infrastructure.Persistence.Models;

namespace PestControl.ContractIntelligence.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();
    public DbSet<DocumentRecord> Documents => Set<DocumentRecord>();
    public DbSet<DocumentPageRecord> DocumentPages => Set<DocumentPageRecord>();
    public DbSet<DocumentChunkRecord> DocumentChunks => Set<DocumentChunkRecord>();
    public DbSet<IngestionJobRecord> IngestionJobs => Set<IngestionJobRecord>();
    public DbSet<AiProviderConfigurationRecord> AiProviderConfigurations => Set<AiProviderConfigurationRecord>();
    public DbSet<ChatSessionRecord> ChatSessions => Set<ChatSessionRecord>();
    public DbSet<ChatMessageRecord> ChatMessages => Set<ChatMessageRecord>();
    public DbSet<MessageSourceRecord> MessageSources => Set<MessageSourceRecord>();
    public DbSet<AuditLogRecord> AuditLogs => Set<AuditLogRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<RefreshTokenRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ContentSha256).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FailureCode).HasMaxLength(100);
            entity.HasIndex(x => new { x.StorageKey, x.Version }).IsUnique();
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DocumentPageRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).IsRequired();
            entity.Property(x => x.ExtractionMethod).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.DocumentId, x.PageNumber }).IsUnique();
            entity.HasOne(x => x.Document).WithMany(x => x.Pages)
                .HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentChunkRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).IsRequired();
            entity.Property(x => x.EmbeddingModel).HasMaxLength(200);
            entity.HasIndex(x => new { x.DocumentId, x.ChunkIndex }).IsUnique();
            entity.HasOne(x => x.Document).WithMany(x => x.Chunks)
                .HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IngestionJobRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FailureCode).HasMaxLength(100);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.HasOne(x => x.Document).WithMany(x => x.IngestionJobs)
                .HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AiProviderConfigurationRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ChatModel).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EmbeddingModel).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.ProviderType).IsUnique();
        });

        modelBuilder.Entity<ChatSessionRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.OwnerId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<ChatMessageRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).IsRequired();
            entity.HasIndex(x => new { x.SessionId, x.CreatedAtUtc });
            entity.HasOne(x => x.Session).WithMany(x => x.Messages)
                .HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageSourceRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Excerpt).IsRequired();
            entity.HasOne(x => x.Message).WithMany(x => x.Sources)
                .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Chunk).WithMany(x => x.MessageSources)
                .HasForeignKey(x => x.ChunkId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLogRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ResourceId).HasMaxLength(100);
            entity.HasIndex(x => new { x.CreatedAtUtc, x.Action });
        });
    }
}
