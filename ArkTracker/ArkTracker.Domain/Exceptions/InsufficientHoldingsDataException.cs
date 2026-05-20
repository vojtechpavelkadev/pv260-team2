namespace ArkTracker.Domain.Exceptions;

public sealed class InsufficientHoldingsDataException : ArkTrackerException
{
    public InsufficientHoldingsDataException(string message)
        : base(message)
    {
    }

    public InsufficientHoldingsDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
