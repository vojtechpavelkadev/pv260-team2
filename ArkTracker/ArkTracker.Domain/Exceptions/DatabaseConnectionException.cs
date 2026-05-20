namespace ArkTracker.Domain.Exceptions;

public sealed class DatabaseConnectionException : ArkTrackerException
{
    public DatabaseConnectionException(string message)
        : base(message)
    {
    }

    public DatabaseConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
