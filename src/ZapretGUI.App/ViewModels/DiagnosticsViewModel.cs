using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.Core.Models;

namespace ZapretGUI.App.ViewModels;

public partial class DiagnosticsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DiagnosticResult> _results = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _lastMessage;

    [ObservableProperty]
    private bool _hasRun;

    public DiagnosticsViewModel()
    {
        RunDiagnosticsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task RunDiagnosticsAsync()
    {
        IsBusy = true;
        try
        {
            var results = await Task.Run(() => AppServices.Diagnostics.Run(AppPaths.BinDir));
            Results = new ObservableCollection<DiagnosticResult>(results);
            HasRun = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task FixTcpTimestampsAsync()
    {
        await RunActionAsync(() =>
        {
            AppServices.Diagnostics.EnableTcpTimestamps();
            LastMessage = "TCP timestamps включены.";
        });
    }

    [RelayCommand]
    private async Task RemoveStrayWinDivertAsync()
    {
        await RunActionAsync(async () =>
        {
            await AppServices.Diagnostics.RemoveStrayWinDivertAsync();
            LastMessage = "Служба WinDivert удалена.";
        });
    }

    [RelayCommand]
    private async Task RemoveConflictingBypassesAsync()
    {
        await RunActionAsync(async () =>
        {
            await AppServices.Diagnostics.RemoveConflictingBypassesAsync();
            LastMessage = "Конфликтующие службы обхода удалены.";
        });
    }

    [RelayCommand]
    private async Task ClearDiscordCacheAsync()
    {
        await RunActionAsync(() =>
        {
            var log = AppServices.Diagnostics.ClearDiscordCache();
            LastMessage = string.Join(" ", log);
        });
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
            await RunDiagnosticsAsync();
        }
    }

    private Task RunActionAsync(Action action) =>
        RunActionAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
}
