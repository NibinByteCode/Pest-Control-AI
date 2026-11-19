namespace PestControl.ContractIntelligence.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
