namespace RecommendationService.Application.Exceptions;

using System;

public class AiServiceException : Exception
{
    public string? UpstreamStatus { get; }
    public string? UpstreamBody { get; }

    protected AiServiceException(string message, string? upstreamStatus = null, string? upstreamBody = null,
        Exception? inner = null)
        : base(message, inner)
    {
        UpstreamStatus = upstreamStatus;
        UpstreamBody = upstreamBody;
    }
}

public sealed class AiTransientException : AiServiceException
{
    public AiTransientException(string message, string? upstreamStatus = null, string? upstreamBody = null,
        Exception? inner = null)
        : base(message, upstreamStatus, upstreamBody, inner)
    {
    }
}

public sealed class AiAuthException : AiServiceException
{
    public AiAuthException(string message, string? upstreamStatus = null, string? upstreamBody = null,
        Exception? inner = null)
        : base(message, upstreamStatus, upstreamBody, inner)
    {
    }
}

public sealed class AiInvalidResponseException : AiServiceException
{
    public AiInvalidResponseException(string message, string? upstreamBody = null, Exception? inner = null)
        : base(message, null, upstreamBody, inner)
    {
    }
}