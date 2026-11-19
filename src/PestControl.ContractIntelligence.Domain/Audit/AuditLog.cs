using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Audit;

public sealed class AuditLog : Entity
{
    private AuditLog(Guid id, Guid? actorId, string action, string resourceType, string? resourceId)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        ActorId = actorId;
        Action = action.Trim();
        ResourceType = resourceType.Trim();
        ResourceId = resourceId?.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid? ActorId { get; }
    public string Action { get; }
    public string ResourceType { get; }
    public string? ResourceId { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static AuditLog Create(Guid? actorId, string action, string resourceType, string? resourceId = null)
        => new(Guid.NewGuid(), actorId, action, resourceType, resourceId);
}
