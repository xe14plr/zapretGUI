using ZapretGUI.Core.Services;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.App.Services;

/// <summary>
/// Simple shared-instance locator for the Core services. Deliberately not a full DI
/// container - the app is small enough that a static holder keeps things simple while
/// pages/viewmodels still depend only on the Core service classes.
/// </summary>
public static class AppServices
{
    public static SettingsStore Settings { get; } = new(AppPaths.ConfigPath);
    public static StrategyCatalog Strategies { get; } = new(AppPaths.StrategiesDir);
    public static ProcessRunner ProcessRunner { get; } = new();
    public static ServiceManager ServiceManager { get; } = new();
    public static IpsetFilterService IpsetFilter { get; } = new();
    public static ListUpdateService ListUpdate { get; } = new();
    public static DiagnosticsService Diagnostics { get; } = new();
    public static UpdateCheckService UpdateCheck { get; } = new();
    public static ModalService Modal { get; } = new();
    public static ServiceHealthCheckService HealthCheck { get; } = new();
    public static StrategyAutoSelectService AutoSelect { get; } = new(HealthCheck);
    public static ThemeService Theme { get; } = new();

    /// <summary>Version of the bundled general*.bat / bin / lists asset set (from upstream's service.bat LOCAL_VERSION).</summary>
    public const string BundledStrategiesVersion = "1.10.0";
}
