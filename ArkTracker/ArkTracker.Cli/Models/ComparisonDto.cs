namespace ArkTracker.Cli.Models;

public class ComparisonResult
{
    public List<SimpleHolding> NewPositions { get; set; } = [];
    public List<HoldingDelta> Increased { get; set; } = [];
    public List<HoldingDelta> Reduced { get; set; } = [];
}

public class SimpleHolding
{
    public string Company { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public long Shares { get; set; }
    public decimal Weight { get; set; }
}

public class HoldingDelta
{
    public string Company { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public long OldShares { get; set; }
    public long NewShares { get; set; }
    public decimal NewWeight { get; set; }

    public bool IsClosed => NewShares == 0;
}
