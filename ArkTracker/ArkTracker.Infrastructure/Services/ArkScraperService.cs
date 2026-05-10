using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Configuration;

namespace ArkTracker.Infrastructure.Services;

public sealed class ArkScraperService(HttpClient httpClient, IConfiguration configuration) : IArkScraperService
{
    public async Task<IEnumerable<HoldingRecord>> DownloadHoldingsAsync()
    {
        string url = configuration["ArkScraper:Url"]
            ?? "https://assets.ark-funds.com/fund-documents/funds-etf-csv/ARK_INNOVATION_ETF_ARKK_HOLDINGS.csv";

        using HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string csvContent = await response.Content.ReadAsStringAsync();
        using StringReader reader = new(csvContent);

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
            TrimOptions = TrimOptions.Trim
        };

        using CsvReader csv = new(reader, config);
        List<ArkHoldingCsvRow> rows = csv.GetRecords<ArkHoldingCsvRow>().ToList();

        DateTime ingestedAtUtc = DateTime.UtcNow;

        return rows.Select(r => new HoldingRecord
        {
            Date = ParseDate(r.Date),
            Fund = NullIfWhiteSpace(r.Fund),
            Company = NullIfWhiteSpace(r.Company),
            Ticker = NullIfWhiteSpace(r.Ticker),
            Cusip = NullIfWhiteSpace(r.Cusip),
            Shares = ParseLong(r.Shares),
            MarketValue = ParseDecimal(r.MarketValue),
            WeightPercentage = ParseDecimal(r.WeightPercentage),
            IngestedAtUtc = ingestedAtUtc
        }).ToList();
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            return null;
        }

        return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }

    private static long? ParseLong(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string sanitized = value.Replace(",", string.Empty).Trim();
        return long.TryParse(sanitized, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            ? parsed
            : null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string sanitized = value
            .Replace("$", string.Empty)
            .Replace("%", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        return decimal.TryParse(sanitized, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsed)
            ? parsed
            : null;
    }

    private class ArkHoldingCsvRow
    {
        public string? Date { get; init; }
        public string? Fund { get; init; }
        public string? Company { get; init; }
        public string? Ticker { get; init; }
        public string? Cusip { get; init; }
        public string? Shares { get; init; }

        [Name("market value ($)")]
        public string? MarketValue { get; init; }

        [Name("weight (%)")]
        public string? WeightPercentage { get; init; }
    }
}
