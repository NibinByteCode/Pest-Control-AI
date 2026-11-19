using System.Diagnostics.CodeAnalysis;

namespace PestControl.ContractIntelligence.Application.Abstractions.Jobs;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Queue is the domain port name defined by the ingestion architecture.")]
public interface IIngestionJobQueue
{
    ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken);
}
