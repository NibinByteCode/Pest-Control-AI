namespace PestControl.ContractIntelligence.Application.Common.Exceptions;

public sealed class AuthorizationException : Exception
{
    public AuthorizationException(string message)
        : base(message)
    {
    }
}
