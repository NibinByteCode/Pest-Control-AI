using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Ai;

public sealed class AiProviderConfiguration : Entity
{
    private AiProviderConfiguration(Guid id, AiProviderType providerType, string chatModel, string embeddingModel)
        : base(id)
    {
        ProviderType = providerType;
        ChatModel = Required(chatModel, nameof(chatModel));
        EmbeddingModel = Required(embeddingModel, nameof(embeddingModel));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public AiProviderType ProviderType { get; }
    public string ChatModel { get; private set; }
    public string EmbeddingModel { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static AiProviderConfiguration Create(AiProviderType providerType, string chatModel, string embeddingModel)
        => new(Guid.NewGuid(), providerType, chatModel, embeddingModel);

    public void Enable() { IsEnabled = true; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public void Disable() { IsEnabled = false; UpdatedAtUtc = DateTimeOffset.UtcNow; }

    public void UpdateModels(string chatModel, string embeddingModel)
    {
        ChatModel = Required(chatModel, nameof(chatModel));
        EmbeddingModel = Required(embeddingModel, nameof(embeddingModel));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
