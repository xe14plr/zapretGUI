namespace ZapretGUI.Core.Models;

public enum ServiceStatus
{
    NotInstalled,
    Stopped,
    StartPending,
    Running,
    StopPending
}

public sealed class ZapretStatus
{
    public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.NotInstalled;
    public bool WinwsProcessRunning { get; set; }
    public string? InstalledStrategyFileName { get; set; }
    public bool WinDivertSysPresent { get; set; }
}
