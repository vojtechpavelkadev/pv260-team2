namespace ArkTracker.Domain.Exceptions;

public abstract class ArkTrackerException : Exception
{
    protected ArkTrackerException(string message)
        : base(message)
    {
    }

    protected ArkTrackerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
