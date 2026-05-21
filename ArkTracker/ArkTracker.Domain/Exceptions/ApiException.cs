namespace ArkTracker.Domain.Exceptions;

public sealed class ApiException : ArkTrackerException
{
    public int? StatusCode { get; }

    public ApiException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException, int? statusCode = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
