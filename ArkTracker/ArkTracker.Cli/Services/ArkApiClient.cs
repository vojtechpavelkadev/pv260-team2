using System.Net.Http.Json;
using ArkTracker.Cli.Models;

namespace ArkTracker.Cli.Services;

public interface IArkApiClient
{
    Task<ComparisonResult?> GetComparisonAsync(DateTime? from = null, DateTime? to = null);
    Task<List<DateTime>> GetAvailableDatesAsync();
}

public class ArkApiClient(HttpClient httpClient) : IArkApiClient
{
    public async Task<ComparisonResult?> GetComparisonAsync(DateTime? from = null, DateTime? to = null)
    {
        var path = "compare";
        if (from.HasValue && to.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc);
            path += $"?from={Uri.EscapeDataString(fromUtc.ToString("O"))}&to={Uri.EscapeDataString(toUtc.ToString("O"))}";
        }

        return await httpClient.GetFromJsonAsync<ComparisonResult>(path);
    }

    public async Task<List<DateTime>> GetAvailableDatesAsync()
    {
        var response = await httpClient.GetFromJsonAsync<DatesResponse>("dates");
        return response?.Dates.ToList() ?? [];
    }

    private record DatesResponse(IEnumerable<DateTime> Dates);
}
