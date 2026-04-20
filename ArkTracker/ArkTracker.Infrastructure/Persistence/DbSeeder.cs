using ArkTracker.Domain.Entities;

namespace ArkTracker.Infrastructure.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Holdings.Any())
            return;

        var data = new List<HoldingRecord>();

        void Add(string date, string ticker, string company, long shares, decimal weight)
        {
            data.Add(new HoldingRecord
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse(date).ToUniversalTime(),
                Fund = "ARKK",
                Ticker = ticker,
                Company = company,
                Shares = shares,
                WeightPercentage = weight
            });
        }

        // DAY 1 - 2026-04-16
        Add("2026-04-16", "TSLA", "Tesla Inc", 1600000, 9.5m);
        Add("2026-04-16", "CRSP", "CRISPR Therapeutics", 7800000, 6.2m);
        Add("2026-04-16", "HOOD", "Robinhood Markets", 3500000, 4.5m);
        Add("2026-04-16", "AMD", "Advanced Micro Devices", 1200000, 4.3m);
        Add("2026-04-16", "SHOP", "Shopify Inc", 2400000, 4.1m);
        Add("2026-04-16", "COIN", "Coinbase", 1500000, 4.0m);
        Add("2026-04-16", "RBLX", "Roblox", 3700000, 3.0m);
        Add("2026-04-16", "PLTR", "Palantir", 1400000, 2.8m);
        Add("2026-04-16", "NVDA", "Nvidia", 390000, 1.2m);
        Add("2026-04-16", "AMZN", "Amazon", 600000, 2.0m);

        // DAY 2 - 2026-04-17
        Add("2026-04-17", "TSLA", "Tesla Inc", 1650000, 9.7m); // increase
        Add("2026-04-17", "CRSP", "CRISPR Therapeutics", 7600000, 6.0m); // decrease
        Add("2026-04-17", "HOOD", "Robinhood Markets", 3500000, 4.5m);
        Add("2026-04-17", "AMD", "Advanced Micro Devices", 1250000, 4.4m);
        Add("2026-04-17", "SHOP", "Shopify Inc", 2300000, 3.9m);
        Add("2026-04-17", "COIN", "Coinbase", 1550000, 4.2m);
        Add("2026-04-17", "RBLX", "Roblox", 3800000, 3.1m);
        Add("2026-04-17", "PLTR", "Palantir", 1400000, 2.8m);
        Add("2026-04-17", "NVDA", "Nvidia", 410000, 1.3m);
        Add("2026-04-17", "META", "Meta Platforms", 35000, 0.3m);

        // DAY 3 - 2026-04-18
        Add("2026-04-18", "TSLA", "Tesla Inc", 1700000, 9.8m);
        Add("2026-04-18", "CRSP", "CRISPR Therapeutics", 7400000, 5.8m);
        Add("2026-04-18", "HOOD", "Robinhood Markets", 3600000, 4.6m);
        Add("2026-04-18", "AMD", "Advanced Micro Devices", 1200000, 4.2m);
        Add("2026-04-18", "SHOP", "Shopify Inc", 2250000, 3.8m);
        Add("2026-04-18", "COIN", "Coinbase", 1600000, 4.3m);
        Add("2026-04-18", "RBLX", "Roblox", 3900000, 3.2m);
        Add("2026-04-18", "PLTR", "Palantir", 1450000, 2.9m);
        Add("2026-04-18", "NVDA", "Nvidia", 420000, 1.35m);
        Add("2026-04-18", "AMZN", "Amazon", 590000, 1.9m);

        // DAY 4 - 2026-04-19
        Add("2026-04-19", "TSLA", "Tesla Inc", 1720000, 9.76m);
        Add("2026-04-19", "CRSP", "CRISPR Therapeutics", 7900000, 6.4m);
        Add("2026-04-19", "HOOD", "Robinhood Markets", 3820000, 4.9m);
        Add("2026-04-19", "AMD", "Advanced Micro Devices", 1150000, 4.3m);
        Add("2026-04-19", "SHOP", "Shopify Inc", 2400000, 4.5m);
        Add("2026-04-19", "COIN", "Coinbase", 1520000, 4.4m);
        Add("2026-04-19", "RBLX", "Roblox", 3750000, 3.1m);
        Add("2026-04-19", "PLTR", "Palantir", 1520000, 3.1m);
        Add("2026-04-19", "NVDA", "Nvidia", 402000, 1.15m);
        Add("2026-04-19", "META", "Meta Platforms", 38000, 0.35m);

        // DAY 5 - 2026-04-20
        Add("2026-04-20", "TSLA", "Tesla Inc", 1723133, 9.76m);
        Add("2026-04-20", "CRSP", "CRISPR Therapeutics", 7873533, 6.46m);
        Add("2026-04-20", "HOOD", "Robinhood Markets", 3827381, 4.91m);
        Add("2026-04-20", "AMD", "Advanced Micro Devices", 1150505, 4.53m);
        Add("2026-04-20", "SHOP", "Shopify Inc", 2439419, 4.52m);
        Add("2026-04-20", "COIN", "Coinbase", 1522394, 4.44m);
        Add("2026-04-20", "RBLX", "Roblox", 3758657, 3.21m);
        Add("2026-04-20", "PLTR", "Palantir", 1521229, 3.15m);
        Add("2026-04-20", "NVDA", "Nvidia", 402002, 1.15m);
        Add("2026-04-20", "AMZN", "Amazon", 597381, 2.12m);

        db.Holdings.AddRange(data);
        db.SaveChanges();
    }
}
