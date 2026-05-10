namespace ArkTracker.Domain.Entities
{
    public class HoldingRecord
    {
        public Guid Id { get; set; }
        public DateTime? Date { get; set; }
        public string? Fund { get; set; }
        public string? Company { get; set; }
        public string? Ticker { get; set; }
        public string? Cusip { get; set; }
        public long? Shares { get; set; }
        public decimal? MarketValue { get; set; }
        public decimal? WeightPercentage { get; set; }
        public DateTime IngestedAtUtc { get; set; }
    }
}
