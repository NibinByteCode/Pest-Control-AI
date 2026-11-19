using PestControl.ContractIntelligence.Application.Features.Authentication;
using PestControl.ContractIntelligence.Application.Features.Chat;
using PestControl.ContractIntelligence.Application.Features.Documents;
using PestControl.ContractIntelligence.Application.Features.Settings;
using PestControl.ContractIntelligence.Application.Features.Users;
using PestControl.ContractIntelligence.Domain.Ai;
using PestControl.ContractIntelligence.Domain.Identity;

namespace PestControl.ContractIntelligence.Application.Tests;

public sealed class RequestValidationTests
{
    [Fact]
    public void BootstrapValidatorRejectsShortPassword()
    {
        var result = AuthenticationRequestValidator.Validate(
            new BootstrapAdminRequest("admin", "admin@example.com", "short"));

        Assert.False(result.IsValid);
        Assert.Contains("Password must be at least 12 characters.", result.Errors);
    }

    [Fact]
    public void DocumentRootValidatorRejectsTraversal()
    {
        var result = DocumentRequestValidator.Validate(
            new RegisterDocumentRootRequest("Contracts", @"\\server\..\secrets"));

        Assert.False(result.IsValid);
        Assert.Contains("Root path must not contain traversal segments.", result.Errors);
    }

    [Fact]
    public void ChatValidatorRejectsMissingSession()
    {
        var result = ChatRequestValidator.Validate(
            new AskQuestionRequest(Guid.Empty, "What is the renewal term?"));

        Assert.False(result.IsValid);
        Assert.Contains("Session ID is required.", result.Errors);
    }

    [Fact]
    public void PagingValidatorRejectsValuesOutsideBounds()
    {
        var result = DocumentRequestValidator.ValidatePaging(0, 101);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void SettingsValidatorRequiresBothModels()
    {
        var result = SettingsRequestValidator.Validate(
            new UpdateAiProviderSettingsRequest(AiProviderType.Ollama, "", "", true));

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void UserValidatorRequiresIdentityAndPassword()
    {
        var result = UserRequestValidator.Validate(
            new CreateUserRequest("", "", "", Role.Viewer));

        Assert.False(result.IsValid);
        Assert.Contains("User name is required.", result.Errors);
        Assert.Contains("Password is required.", result.Errors);
    }
}
