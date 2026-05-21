using System.Net.Http.Json;
using System.Text.Json;
using ArkTracker.Cli.Models;
using ArkTracker.Domain.Exceptions;

namespace ArkTracker.Cli.Services;

public interface IArkApiClient
{
    Task<string?> LoginAsync(string username, string password);
    Task<ComparisonResult?> GetComparisonAsync(DateTime? from = null, DateTime? to = null);
    Task<List<DateTime>> GetAvailableDatesAsync();
}

public class ArkApiClient(HttpClient httpClient) : IArkApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<string?> LoginAsync(string username, string password)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("auth/login", new { username, password });
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        await EnsureSuccessOrThrowAsync(response);

        LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
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
        string path = "holdings/compare";
        if (from.HasValue && to.HasValue)
        {
            DateTime fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
            DateTime toUtc = DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc);
            path += $"?from={Uri.EscapeDataString(fromUtc.ToString("O"))}&to={Uri.EscapeDataString(toUtc.ToString("O"))}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(path);
        await EnsureSuccessOrThrowAsync(response);
        return await response.Content.ReadFromJsonAsync<ComparisonResult>();
    }

    public async Task<List<DateTime>> GetAvailableDatesAsync()
    {
        HttpResponseMessage response = await httpClient.GetAsync("holdings/dates");
        await EnsureSuccessOrThrowAsync(response);
        DatesResponse? datesResponse = await response.Content.ReadFromJsonAsync<DatesResponse>();
        return datesResponse?.Dates.ToList() ?? [];
    }

    private record DatesResponse(IEnumerable<DateTime> Dates);

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorMessage = await ReadErrorMessageAsync(response);
        int statusCode = (int)response.StatusCode;

        if (statusCode == 404 &&
            errorMessage.Contains("Not enough data to compare", StringComparison.OrdinalIgnoreCase))
        {
            throw new InsufficientHoldingsDataException(errorMessage);
        }

        if (statusCode == 400)
        {
            throw new DomainValidationException(errorMessage);
        }

        throw new ApiException(errorMessage, statusCode);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Request failed with status {(int)response.StatusCode}.";
        }

        try
        {
            ErrorResponse? errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(errorResponse?.Error))
            {
                return errorResponse.Error;
            }
        }
        catch (JsonException)
        {
        }

        return body;
    }

    private sealed record ErrorResponse(string? Error);
}
