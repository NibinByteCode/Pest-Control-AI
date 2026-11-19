using PestControl.ContractIntelligence.Domain.Common;

namespace PestControl.ContractIntelligence.Domain.Documents;

public sealed class DocumentPage : Entity
{
    private DocumentPage(Guid id, Guid documentId, int pageNumber, string text, string extractionMethod)
        : base(id)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document ID must not be empty.", nameof(documentId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(extractionMethod);

        DocumentId = documentId;
        PageNumber = pageNumber;
        Text = text ?? string.Empty;
        ExtractionMethod = extractionMethod.Trim();
    }

    public Guid DocumentId { get; }
    public int PageNumber { get; }
    public string Text { get; }
    public string ExtractionMethod { get; }

    public static DocumentPage Create(Guid documentId, int pageNumber, string text, string extractionMethod)
        => new(Guid.NewGuid(), documentId, pageNumber, text, extractionMethod);
}
