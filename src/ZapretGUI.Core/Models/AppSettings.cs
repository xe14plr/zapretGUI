namespace ZapretGUI.Core.Models;

public enum GameFilterMode
{
    Disabled,
    All,
    Tcp,
    Udp
}

public enum IpsetFilterMode
{
    None,
    Loaded,
    Any
}

public sealed class AppSettings
{
    public GameFilterMode GameFilter { get; set; } = GameFilterMode.Disabled;
    public IpsetFilterMode IpsetFilter { get; set; } = IpsetFilterMode.Loaded;
    public bool AutoUpdateCheckEnabled { get; set; } = true;
    public string? ActiveStrategyFileName { get; set; }
}
