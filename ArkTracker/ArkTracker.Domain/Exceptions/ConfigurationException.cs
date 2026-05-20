namespace ArkTracker.Domain.Exceptions;

public sealed class ConfigurationException : ArkTrackerException
{
    public ConfigurationException(string message)
        : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
