using System.Net.Http.Json;
using ArkTracker.Cli.Models;

namespace ArkTracker.Cli.Services;

public interface IArkApiClient
{
    Task<string?> LoginAsync(string username, string password);
    Task<ComparisonResult?> GetComparisonAsync(DateTime? from = null, DateTime? to = null);
    Task<List<DateTime>> GetAvailableDatesAsync();
}

public class ArkApiClient(HttpClient httpClient) : IArkApiClient
{
    public async Task<string?> LoginAsync(string username, string password)
    {
        var response = await httpClient.PostAsJsonAsync("auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.Token != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            return result.Token;
        }
        return null;
    }

    private record LoginResponse(string Token);

    public async Task<ComparisonResult?> GetComparisonAsync(DateTime? from = null, DateTime? to = null)
    {
        var path = "holdings/compare";
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
        var response = await httpClient.GetFromJsonAsync<DatesResponse>("holdings/dates");
        return response?.Dates.ToList() ?? [];
    }

    private record DatesResponse(IEnumerable<DateTime> Dates);
}
