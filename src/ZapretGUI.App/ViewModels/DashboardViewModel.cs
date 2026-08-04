using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.Core.Models;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<StrategyInfo> _strategies = new();

    [ObservableProperty]
    private StrategyInfo? _selectedStrategy;

    [ObservableProperty]
    private bool _isServiceInstalled;

    [ObservableProperty]
    private bool _isServiceRunning;

    [ObservableProperty]
    private bool _isWinwsRunning;

    [ObservableProperty]
    private string _installedStrategyDisplay = "—";

    [ObservableProperty]
    private string _statusText = "Проверка статуса...";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _lastError;

    public DashboardViewModel()
    {
        foreach (var s in AppServices.Strategies.GetStrategies())
        {
            Strategies.Add(s);
        }

        var settings = AppServices.Settings.Load();
        SelectedStrategy = Strategies.FirstOrDefault(s => s.FileName == settings.ActiveStrategyFileName)
                            ?? Strategies.FirstOrDefault();

        Refresh();
    }

    [RelayCommand]
    private void Refresh()
    {
        var status = AppServices.ServiceManager.GetStatus(AppPaths.BinDir);

        IsServiceInstalled = status.ServiceStatus != ServiceStatus.NotInstalled;
        IsServiceRunning = status.ServiceStatus == ServiceStatus.Running;
        IsWinwsRunning = status.WinwsProcessRunning || AppServices.ProcessRunner.IsRunning;
        InstalledStrategyDisplay = status.InstalledStrategyFileName ?? "—";

        StatusText = IsWinwsRunning
            ? (IsServiceRunning ? "Служба запущена, обход активен" : "Обход запущен вручную")
            : "Обход не запущен";
    }

    [RelayCommand]
    private async Task RunStandaloneAsync()
    {
        if (SelectedStrategy is null)
        {
            return;
        }

        var strategy = SelectedStrategy;
        await RunOperationAsync(() =>
        {
            var settings = AppServices.Settings.Load();
            var resolved = StrategyArgsBuilder.Build(strategy, AppPaths.BinDir, AppPaths.ListsDir, settings.GameFilter);
            AppServices.ProcessRunner.Start(resolved);
        });
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await RunOperationAsync(() =>
        {
            AppServices.ProcessRunner.Stop();
            Core.Services.ProcessRunner.KillAllWinwsProcesses();
        });
    }

    [RelayCommand]
    private async Task InstallServiceAsync()
    {
        if (SelectedStrategy is null)
        {
            return;
        }

        var strategy = SelectedStrategy;
        await RunOperationAsync(async () =>
        {
            var settings = AppServices.Settings.Load();
            var resolved = StrategyArgsBuilder.Build(strategy, AppPaths.BinDir, AppPaths.ListsDir, settings.GameFilter);
            await AppServices.ServiceManager.InstallAsync(resolved, strategy.FileName);
            settings.ActiveStrategyFileName = strategy.FileName;
            AppServices.Settings.Save(settings);
        });
    }

    [RelayCommand]
    private async Task RemoveServiceAsync()
    {
        await RunOperationAsync(async () => await AppServices.ServiceManager.RemoveAsync());
    }

    private async Task RunOperationAsync(Func<Task> operation)
    {
        IsBusy = true;
        LastError = null;
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
        finally
        {
            IsBusy = false;
            Refresh();
        }
    }

    private Task RunOperationAsync(Action operation) =>
        RunOperationAsync(() =>
        {
            operation();
            return Task.CompletedTask;
        });
}
