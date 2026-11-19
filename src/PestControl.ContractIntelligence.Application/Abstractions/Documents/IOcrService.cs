namespace PestControl.ContractIntelligence.Application.Abstractions.Documents;

public interface IOcrService
{
    Task<OcrPageResult> RecognizeAsync(Stream image, CancellationToken cancellationToken);
}

public sealed record OcrPageResult(string Text, double? Confidence, string Engine, string? Diagnostic);
