namespace PestControl.ContractIntelligence.Application.Abstractions.Ai;

public interface IEmbeddingProvider
{
    string ProviderKey { get; }
    Task<EmbeddingResult> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken);
}

public sealed record EmbeddingResult(
    string Model,
    int Dimension,
    IReadOnlyList<IReadOnlyList<float>> Vectors);
