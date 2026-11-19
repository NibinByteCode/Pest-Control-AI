namespace PestControl.ContractIntelligence.Application.Abstractions.Documents;

public interface IDocumentTextExtractor
{
    Task<ExtractedDocument> ExtractAsync(Stream pdf, CancellationToken cancellationToken);
}

public sealed record ExtractedDocument(IReadOnlyList<ExtractedPage> Pages);
public sealed record ExtractedPage(int PageNumber, string Text, string ExtractionMethod, string? Diagnostic);
