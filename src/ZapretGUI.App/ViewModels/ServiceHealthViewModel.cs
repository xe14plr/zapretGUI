using CommunityToolkit.Mvvm.ComponentModel;

namespace ZapretGUI.App.ViewModels;

public partial class ServiceHealthViewModel : ObservableObject
{
    public required string Name { get; init; }

    /// <summary>null while the first check is still running.</summary>
    [ObservableProperty]
    private bool? _isReachable;
}
