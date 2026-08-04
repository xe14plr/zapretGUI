using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Resolves the %GameFilter%/%GameFilterTCP%/%GameFilterUDP% tokens the same way
/// service.bat's :game_switch_status does.
/// </summary>
public static class GameFilterService
{
    private const string DisabledPort = "12";
    private const string EnabledRange = "1024-65535";

    public static (string GameFilter, string GameFilterTcp, string GameFilterUdp) Resolve(GameFilterMode mode) => mode switch
    {
        GameFilterMode.Disabled => (DisabledPort, DisabledPort, DisabledPort),
        GameFilterMode.All => (EnabledRange, EnabledRange, EnabledRange),
        GameFilterMode.Tcp => (EnabledRange, EnabledRange, DisabledPort),
        GameFilterMode.Udp => (EnabledRange, DisabledPort, EnabledRange),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}
