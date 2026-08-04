using ZapretGUI.Core.Models;
using ZapretGUI.Core.Services;

namespace ZapretGUI.Core.Strategies;

public sealed class ResolvedStrategy
{
    public required string ExecutablePath { get; init; }
    public required string Arguments { get; init; }
}

/// <summary>
/// Ties BatArgsParser + GameFilterService together to produce a ready-to-run
/// winws.exe invocation for a given strategy and current settings.
/// </summary>
public static class StrategyArgsBuilder
{
    public static ResolvedStrategy Build(StrategyInfo strategy, string binDir, string listsDir, GameFilterMode gameFilterMode)
    {
        var template = BatArgsParser.ExtractRawArgsTemplate(strategy.FilePath);
        var (gameFilter, gameFilterTcp, gameFilterUdp) = GameFilterService.Resolve(gameFilterMode);
        var arguments = BatArgsParser.Substitute(template, binDir, listsDir, gameFilter, gameFilterTcp, gameFilterUdp);

        return new ResolvedStrategy
        {
            ExecutablePath = Path.Combine(binDir, "winws.exe"),
            Arguments = arguments
        };
    }
}
