namespace ZapretGUI.Core.Models;

public sealed class ServiceHealth
{
    public required string GroupName { get; init; }
    public bool IsReachable { get; init; }
}
