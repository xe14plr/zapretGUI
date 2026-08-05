using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.App.Views.Overlays;
using ZapretGUI.Core.Models;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DispatcherTimer _healthTimer;

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

    [ObservableProperty]
    private string? _lastMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RunButtonLabel))]
    private bool _isAutoMode = true;

    public string RunButtonLabel => IsAutoMode ? "Подобрать и запустить" : "Запустить вручную";

    public ObservableCollection<ServiceHealthViewModel> ServiceHealth { get; } = new()
    {
        new ServiceHealthViewModel { Name = "Discord" },
        new ServiceHealthViewModel { Name = "YouTube" }
    };

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
        _ = RefreshHealthAsync();

        _healthTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _healthTimer.Tick += async (_, _) => await RefreshHealthAsync();
        _healthTimer.Start();
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

    private async Task RefreshHealthAsync()
    {
        try
        {
            var results = await AppServices.HealthCheck.CheckAllAsync();
            foreach (var result in results)
            {
                var vm = ServiceHealth.FirstOrDefault(h => h.Name == result.GroupName);
                if (vm is not null)
                {
                    vm.IsReachable = result.IsReachable;
                }
            }
        }
        catch
        {
            // best-effort; leave chips in their last known state
        }
    }

    [RelayCommand]
    private void SetAutoMode() => IsAutoMode = true;

    [RelayCommand]
    private void SetManualMode() => IsAutoMode = false;

    [RelayCommand]
    private void OpenStrategyDrawer() =>
        AppServices.Modal.Show(new StrategyDrawerView { DataContext = this }, ModalPlacement.Bottom);

    [RelayCommand]
    private async Task RunAsync()
    {
        if (IsAutoMode)
        {
            await RunAutoSelectAsync();
        }
        else
        {
            await RunStandaloneAsync();
        }
    }

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
            LastMessage = $"Запущено: {strategy.DisplayName}";
        });
    }

    private async Task RunAutoSelectAsync()
    {
        if (Strategies.Count == 0)
        {
            return;
        }

        await RunOperationAsync(async () =>
        {
            var settings = AppServices.Settings.Load();
            var progress = new Progress<AutoSelectProgress>(p =>
                StatusText = $"Тестирую «{p.StrategyName}»… ({p.Current}/{p.Total})");

            var result = await AppServices.AutoSelect.RunAsync(
                Strategies, AppPaths.BinDir, AppPaths.ListsDir, settings.GameFilter, progress);

            if (result.WinningStrategy is not null)
            {
                var winner = Strategies.FirstOrDefault(s => s.FileName == result.WinningStrategy.FileName);
                if (winner is not null)
                {
                    SelectedStrategy = winner;
                    var resolved = StrategyArgsBuilder.Build(winner, AppPaths.BinDir, AppPaths.ListsDir, settings.GameFilter);
                    AppServices.ProcessRunner.Start(resolved);
                    LastMessage = $"Подобрана стратегия «{winner.DisplayName}» ({result.ReachableGroups}/{result.TotalGroups} сервисов доступно)";
                }
            }
            else
            {
                LastError = "Не удалось подобрать рабочую стратегию. Попробуйте вручную или откройте Диагностику.";
            }

            await RefreshHealthAsync();
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
        LastMessage = null;
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
