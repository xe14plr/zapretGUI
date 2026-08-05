using ZapretGUI.Core.Models;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Cycles through the bundled strategies, running each standalone for a short settle window
/// and scoring it against ServiceHealthCheckService, mirroring the connectivity-testing idea
/// from upstream's utils/test zapret.ps1 but scoped to Discord/YouTube reachability.
/// </summary>
public sealed class StrategyAutoSelectService
{
    private readonly ServiceHealthCheckService _healthCheck;

    public StrategyAutoSelectService(ServiceHealthCheckService? healthCheck = null)
    {
        _healthCheck = healthCheck ?? new ServiceHealthCheckService();
    }

    public async Task<AutoSelectResult> RunAsync(
        IReadOnlyList<StrategyInfo> strategies,
        string binDir,
        string listsDir,
        GameFilterMode gameFilterMode,
        IProgress<AutoSelectProgress>? progress = null,
        CancellationToken ct = default)
    {
        StrategyInfo? best = null;
        var bestScore = -1;
        var totalGroups = 0;

        var runner = new ProcessRunner();

        try
        {
            for (var i = 0; i < strategies.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var strategy = strategies[i];
                progress?.Report(new AutoSelectProgress
                {
                    Current = i + 1,
                    Total = strategies.Count,
                    StrategyName = strategy.DisplayName
                });

                try
                {
                    var resolved = StrategyArgsBuilder.Build(strategy, binDir, listsDir, gameFilterMode);
                    runner.Start(resolved);

                    await Task.Delay(1500, ct);

                    var healthResults = await _healthCheck.CheckAllAsync(ct);
                    totalGroups = healthResults.Count;
                    var score = healthResults.Count(h => h.IsReachable);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = strategy;
                    }

                    runner.Stop();

                    if (totalGroups > 0 && score == totalGroups)
                    {
                        break;
                    }
                }
                catch (Exception) when (ct.IsCancellationRequested == false)
                {
                    runner.Stop();
                }
            }
        }
        finally
        {
            runner.Stop();
        }

        return new AutoSelectResult
        {
            WinningStrategy = best,
            ReachableGroups = Math.Max(bestScore, 0),
            TotalGroups = totalGroups
        };
    }
}
