using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.Core.Models;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.App.ViewModels;

public partial class StrategiesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<StrategyInfo> _strategies = new();

    [ObservableProperty]
    private StrategyInfo? _selectedStrategy;

    [ObservableProperty]
    private string _generatedCommandLine = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _lastError;

    [ObservableProperty]
    private string? _lastMessage;

    public StrategiesViewModel()
    {
        foreach (var s in AppServices.Strategies.GetStrategies())
        {
            Strategies.Add(s);
        }

        SelectedStrategy = Strategies.FirstOrDefault();
        UpdateGeneratedCommandLine();
    }

    partial void OnSelectedStrategyChanged(StrategyInfo? value) => UpdateGeneratedCommandLine();

    private void UpdateGeneratedCommandLine()
    {
        if (SelectedStrategy is null)
        {
            GeneratedCommandLine = string.Empty;
            return;
        }

        try
        {
            var settings = AppServices.Settings.Load();
            var resolved = StrategyArgsBuilder.Build(SelectedStrategy, AppPaths.BinDir, AppPaths.ListsDir, settings.GameFilter);
            GeneratedCommandLine = $"\"{resolved.ExecutablePath}\" {resolved.Arguments}";
        }
        catch (Exception ex)
        {
            GeneratedCommandLine = $"Ошибка разбора стратегии: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RunAsync()
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
            LastMessage = $"Установлено как служба: {strategy.DisplayName}";
        });
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
        }
    }

    private Task RunOperationAsync(Action operation) =>
        RunOperationAsync(() =>
        {
            operation();
            return Task.CompletedTask;
        });
}
