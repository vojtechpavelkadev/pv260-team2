using ArkTracker.Domain.Exceptions;
using Spectre.Console;

namespace ArkTracker.Cli.UI;

internal static class CliExceptionHandler
{
    public static void ShowCliError(Exception ex, string? contextSuffix = null)
    {
        string message = ex switch
        {
            ArkTrackerException arkEx => arkEx.Message,
            HttpRequestException => "Cannot reach the API. Please ensure it is running and try again.",
            TaskCanceledException => "Request timed out. Please try again.",
            _ => "An unexpected error occurred."
        };

        if (!string.IsNullOrEmpty(contextSuffix))
        {
            message = $"{contextSuffix}: {message}";
        }

        Display.ShowError(message);
    }
}
