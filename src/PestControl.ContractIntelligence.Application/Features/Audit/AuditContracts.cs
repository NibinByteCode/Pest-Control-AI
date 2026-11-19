namespace PestControl.ContractIntelligence.Application.Features.Audit;

public sealed record AuditEventSummary(Guid Id, Guid? ActorId, string Action, string ResourceType, string? ResourceId, DateTimeOffset CreatedAtUtc);

public interface IAuditService
{
    Task<IReadOnlyList<AuditEventSummary>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
}
