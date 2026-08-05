namespace ZapretGUI.Core.Models;

public sealed class AutoSelectProgress
{
    public required int Current { get; init; }
    public required int Total { get; init; }
    public required string StrategyName { get; init; }
}

public sealed class AutoSelectResult
{
    public StrategyInfo? WinningStrategy { get; init; }
    public int ReachableGroups { get; init; }
    public int TotalGroups { get; init; }
}
