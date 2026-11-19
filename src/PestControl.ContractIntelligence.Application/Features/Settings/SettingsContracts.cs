using PestControl.ContractIntelligence.Application.Common.Validation;
using PestControl.ContractIntelligence.Domain.Ai;

namespace PestControl.ContractIntelligence.Application.Features.Settings;

public sealed record AiProviderSettingsSummary(AiProviderType ProviderType, string ChatModel, string EmbeddingModel, bool IsEnabled);
public sealed record UpdateAiProviderSettingsRequest(AiProviderType ProviderType, string ChatModel, string EmbeddingModel, bool IsEnabled);
public sealed record ProviderConnectionResult(bool IsAvailable, string? Diagnostic);

public interface ISettingsService
{
    Task<IReadOnlyList<AiProviderSettingsSummary>> ListAiProvidersAsync(CancellationToken cancellationToken);
    Task<AiProviderSettingsSummary> UpdateAiProviderAsync(UpdateAiProviderSettingsRequest request, CancellationToken cancellationToken);
    Task<ProviderConnectionResult> TestProviderAsync(AiProviderType providerType, CancellationToken cancellationToken);
}

public static class SettingsRequestValidator
{
    public static ValidationResult Validate(UpdateAiProviderSettingsRequest request)
    {
        var result = new ValidationResult();
        result.AddIf(string.IsNullOrWhiteSpace(request.ChatModel), "Chat model is required.");
        result.AddIf(string.IsNullOrWhiteSpace(request.EmbeddingModel), "Embedding model is required.");
        return result;
    }
}
