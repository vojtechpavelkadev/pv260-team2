namespace ArkTracker.Application.CompareHoldings;

public class CompareHoldingsResult
{
    public List<SimpleHolding> NewPositions { get; set; } = new();
    public List<HoldingDelta> Increased { get; set; } = new();
    public List<HoldingDelta> Reduced { get; set; } = new();
}

public class SimpleHolding
{
    public string Company { get; set; } = default!;
    public string Ticker { get; set; } = default!;
    public long Shares { get; set; }
    public decimal Weight { get; set; }
}

public class HoldingDelta
{
    public string Company { get; set; } = default!;
    public string Ticker { get; set; } = default!;
    public long OldShares { get; set; }
    public long NewShares { get; set; }
    public decimal NewWeight { get; set; }

    public bool IsClosed => NewShares == 0;
}