using ArkTracker.Domain.Exceptions;

namespace ArkTracker.Domain.Entities
{
    public class HoldingRecord
    {
        public Guid Id { get; private set; }
        public DateTime? Date { get; private set; }
        public string? Fund { get; private set; }
        public string? Company { get; private set; }
        public string? Ticker { get; private set; }
        public string? Cusip { get; private set; }
        public long? Shares { get; private set; }
        public decimal? MarketValue { get; private set; }
        public decimal? WeightPercentage { get; private set; }
        public DateTime IngestedAtUtc { get; private set; }

        private HoldingRecord() { }

        public HoldingRecord(
            DateTime? date,
            string? fund,
            string? company,
            string? ticker,
            string? cusip,
            long? shares,
            decimal? marketValue,
            decimal? weightPercentage)
        {
            if (shares.HasValue && shares.Value < 0)
                throw new DomainValidationException("Number of shares cannot be negative.");

            if (date.HasValue && date.Value < DateTime.UtcNow.AddYears(-3))
                throw new DomainValidationException("Date cannot be older than 3 years.");

            Id = Guid.NewGuid();
            Date = date;
            Fund = fund;
            Company = company;
            Ticker = ticker;
            Cusip = cusip;
            Shares = shares;
            MarketValue = marketValue;
            WeightPercentage = weightPercentage;
            IngestedAtUtc = DateTime.UtcNow;
        }

        public void UpdateShares(long newShares)
        {
            if (newShares < 0)
                throw new DomainValidationException("Number of shares cannot be negative.");
            Shares = newShares;
        }

        public void UpdateMarketValue(decimal newMarketValue)
        {
            MarketValue = newMarketValue;
        }

        public void UpdateWeightPercentage(decimal newWeightPercentage)
        {
            WeightPercentage = newWeightPercentage;
        }
    }
}
