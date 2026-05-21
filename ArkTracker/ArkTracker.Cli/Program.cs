using ArkTracker.Cli.Models;
using ArkTracker.Cli.Services;
using ArkTracker.Cli.UI;
using ArkTracker.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

string environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
    "Production";

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

ServiceCollection services = new();
services.AddHttpClient<IArkApiClient, ArkApiClient>(client =>
{
    string baseUrl = configuration["ArkApi:BaseUrl"] ?? "http://localhost:5185/api/";
    client.BaseAddress = new Uri(baseUrl);
});

using ServiceProvider serviceProvider = services.BuildServiceProvider();
IArkApiClient apiClient = serviceProvider.GetRequiredService<IArkApiClient>();

string currentSortBy = "Ticker";
ComparisonResult? lastResult = null;
string lastTitle = "Latest Portfolio Changes (LTM)";

Display.ShowWelcomeScreen();

bool authenticated = false;
while (!authenticated)
{
    (string? username, string? password) = Display.PromptForLogin();

    // Allow exit if username is empty
    if (string.IsNullOrWhiteSpace(username))
    {
        AnsiConsole.MarkupLine("[grey]Exiting...[/]");
        return;
    }

    try
    {
        string? token = await apiClient.LoginAsync(username, password);
        if (token != null)
        {
            authenticated = true;
            AnsiConsole.MarkupLine("[green]Authentication successful![/]");
            await Task.Delay(1000);
        }
        else
        {
            Display.ShowError("Invalid credentials. Please try again.");
        }
    }
    catch (ArkTrackerException ex)
    {
        CliExceptionHandler.ShowCliError(ex, "Login failed");
        AnsiConsole.MarkupLine("[grey]Please ensure the API is running and try again, or press Enter on username to exit.[/]");
    }
    catch (HttpRequestException ex)
    {
        CliExceptionHandler.ShowCliError(ex, "Login failed");
        AnsiConsole.MarkupLine("[grey]Please ensure the API is running and try again, or press Enter on username to exit.[/]");
    }
    catch (TaskCanceledException ex)
    {
        CliExceptionHandler.ShowCliError(ex, "Login failed");
        AnsiConsole.MarkupLine("[grey]Please ensure the API is running and try again, or press Enter on username to exit.[/]");
    }
}

try
{
    lastResult = await Display.ShowSplashScreen(() => apiClient.GetComparisonAsync());
}
catch (ArkTrackerException ex)
{
    CliExceptionHandler.ShowCliError(ex, "Initial fetch failed");
}
catch (HttpRequestException ex)
{
    CliExceptionHandler.ShowCliError(ex, "Initial fetch failed");
}
catch (TaskCanceledException ex)
{
    CliExceptionHandler.ShowCliError(ex, "Initial fetch failed");
}

if (lastResult != null)
{
    Display.RenderHeader();
    Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
}

while (true)
{
    string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[grey]What would you like to do?[/]")
            .AddChoices(["Compare Specific Dates", "Change Sort Order", "Refresh Latest", "Exit"]));

    if (choice == "Exit")
    {
        break;
    }

    if (choice == "Change Sort Order")
    {
        currentSortBy = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort by:")
                .AddChoices(["Ticker", "Company", "Weight", "Change Amount"]));

        Display.RenderHeader();
        if (lastResult != null)
        {
            Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
        }

        continue;
    }

    if (choice == "Refresh Latest")
    {
        try
        {
            lastResult = await Display.ShowSplashScreen(() => apiClient.GetComparisonAsync());
            lastTitle = "Latest Portfolio Changes (LTM)";

            if (lastResult != null)
            {
                Display.RenderHeader();
                Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
            }
        }
        catch (ArkTrackerException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Refresh failed");
        }
        catch (HttpRequestException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Refresh failed");
        }
        catch (TaskCanceledException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Refresh failed");
        }
        continue;
    }

    if (choice == "Compare Specific Dates")
    {
        try
        {
            List<DateTime> availableDates = await apiClient.GetAvailableDatesAsync();
            (DateTime from, DateTime to)? selected = Display.PromptForDates(availableDates);

            if (selected.HasValue)
            {
                lastResult = await Display.ShowSplashScreen(() =>
                    apiClient.GetComparisonAsync(selected.Value.from, selected.Value.to));

                lastTitle = $"Comparison: {selected.Value.from:dd. MM. yyyy} vs {selected.Value.to:dd. MM. yyyy}";

                if (lastResult != null)
                {
                    Display.RenderHeader();
                    Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
                }
            }
        }
        catch (ArkTrackerException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Operation failed");
        }
        catch (HttpRequestException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Operation failed");
        }
        catch (TaskCanceledException ex)
        {
            CliExceptionHandler.ShowCliError(ex, "Operation failed");
        }
    }
}

AnsiConsole.MarkupLine("[grey]Thank you for using ArkTracker. Happy investing![/]");