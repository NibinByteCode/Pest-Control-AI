using PestControl.ContractIntelligence.Application.Common.Validation;

namespace PestControl.ContractIntelligence.Application.Features.Chat;

public sealed record CreateChatSessionRequest(string Title);
public sealed record ChatSessionSummary(Guid Id, string Title, DateTimeOffset CreatedAtUtc);
public sealed record AskQuestionRequest(Guid SessionId, string Question);
public sealed record Citation(Guid ChunkId, Guid DocumentId, string DocumentName, int StartPage, int EndPage, string Excerpt);
public sealed record AskQuestionResponse(Guid MessageId, string Answer, IReadOnlyList<Citation> Citations);
public sealed record ChatMessageSummary(Guid Id, string Role, string Content, DateTimeOffset CreatedAtUtc);

public interface IChatService
{
    Task<ChatSessionSummary> CreateSessionAsync(CreateChatSessionRequest request, CancellationToken cancellationToken);
    Task<AskQuestionResponse> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessageSummary>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken);
}

public static class ChatRequestValidator
{
    public static ValidationResult Validate(CreateChatSessionRequest request)
    {
        var result = new ValidationResult();
        result.AddIf(string.IsNullOrWhiteSpace(request.Title), "Title is required.");
        result.AddIf(!string.IsNullOrWhiteSpace(request.Title) && request.Title.Trim().Length > 200, "Title must not exceed 200 characters.");
        return result;
    }

    public static ValidationResult Validate(AskQuestionRequest request)
    {
        var result = new ValidationResult();
        result.AddIf(request.SessionId == Guid.Empty, "Session ID is required.");
        result.AddIf(string.IsNullOrWhiteSpace(request.Question), "Question is required.");
        result.AddIf(!string.IsNullOrWhiteSpace(request.Question) && request.Question.Trim().Length > 4000, "Question must not exceed 4000 characters.");
        return result;
    }
}
