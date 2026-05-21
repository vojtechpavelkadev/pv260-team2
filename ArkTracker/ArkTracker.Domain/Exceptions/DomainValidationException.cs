namespace ArkTracker.Domain.Exceptions;

public sealed class DomainValidationException : ArkTrackerException
{
    public DomainValidationException(string message)
        : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
