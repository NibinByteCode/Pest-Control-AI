namespace PestControl.ContractIntelligence.Application.Abstractions.Ai;

public interface IChatCompletionProvider
{
    string ProviderKey { get; }
    Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken);
}

public sealed record ChatCompletionRequest(
    string SystemInstruction,
    string UserQuestion,
    IReadOnlyList<ChatEvidence> Evidence);

public sealed record ChatEvidence(
    Guid ChunkId,
    Guid DocumentId,
    int StartPage,
    int EndPage,
    string Text);

public sealed record ChatCompletionResult(string Content, string Model, int PromptTokens, int CompletionTokens);
