using ArkTracker.Cli.Models;
using ArkTracker.Cli.Services;
using ArkTracker.Cli.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddHttpClient<IArkApiClient, ArkApiClient>(client => 
{
    var baseUrl = configuration["ArkApi:BaseUrl"] ?? "http://localhost:5185/api/holdings/";
    client.BaseAddress = new Uri(baseUrl);
});

using var serviceProvider = services.BuildServiceProvider();
var apiClient = serviceProvider.GetRequiredService<IArkApiClient>();

string currentSortBy = "Ticker";
ComparisonResult? lastResult = null;
string lastTitle = "Latest Portfolio Changes (LTM)";

Display.ShowWelcomeScreen();

try
{
    lastResult = await Display.ShowSplashScreen(() => apiClient.GetComparisonAsync());
}
catch (Exception ex)
{
    Display.ShowError($"Initial fetch failed: {ex.Message}");
}

if (lastResult != null)
{
    Display.RenderHeader();
    Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
}

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[grey]What would you like to do?[/]")
            .AddChoices(["Compare Specific Dates", "Change Sort Order", "Refresh Latest", "Exit"]));

    if (choice == "Exit") break;

    if (choice == "Change Sort Order")
    {
        currentSortBy = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort by:")
                .AddChoices(["Ticker", "Company", "Weight", "Change Amount"]));
        
        Display.RenderHeader();
        if (lastResult != null) Display.RenderComparisonTable(lastResult, lastTitle, currentSortBy);
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
        catch (Exception ex)
        {
            Display.ShowError($"Refresh failed: {ex.Message}");
        }
        continue;
    }

    if (choice == "Compare Specific Dates")
    {
        try
        {
            var availableDates = await apiClient.GetAvailableDatesAsync();
            var selected = Display.PromptForDates(availableDates);
            
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
        catch (Exception ex)
        {
            Display.ShowError($"Operation failed: {ex.Message}");
        }
    }
}

AnsiConsole.MarkupLine("[grey]Thank you for using ArkTracker. Happy investing![/]");