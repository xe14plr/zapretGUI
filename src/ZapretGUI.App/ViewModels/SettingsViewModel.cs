using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.App.Views.Overlays;
using ZapretGUI.Core.Models;

namespace ZapretGUI.App.ViewModels;

public sealed record EnumOption<T>(T Value, string Label);

public partial class SettingsViewModel : ObservableObject
{
    private readonly string _ipsetAllPath = Path.Combine(AppPaths.ListsDir, "ipset-all.txt");
    private bool _loaded;

    public IReadOnlyList<EnumOption<GameFilterMode>> GameFilterOptions { get; } =
    [
        new(GameFilterMode.Disabled, "Отключено"),
        new(GameFilterMode.All, "Включено (TCP и UDP)"),
        new(GameFilterMode.Tcp, "Включено (только TCP)"),
        new(GameFilterMode.Udp, "Включено (только UDP)")
    ];

    [ObservableProperty]
    private GameFilterMode _gameFilter;

    [ObservableProperty]
    private IpsetFilterMode _ipsetFilter;

    [ObservableProperty]
    private string _ipsetFilterDescription = string.Empty;

    [ObservableProperty]
    private bool _autoUpdateEnabled;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _lastError;

    [ObservableProperty]
    private string? _lastMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    [NotifyPropertyChangedFor(nameof(IsLightTheme))]
    private AppTheme _theme;

    [ObservableProperty]
    private string _accentColorHex = "#8B5CF6";

    public bool IsDarkTheme => Theme == AppTheme.Dark;
    public bool IsLightTheme => Theme == AppTheme.Light;

    public IReadOnlyList<string> AccentSwatches { get; } = ThemeService.AccentSwatches;

    public SettingsViewModel()
    {
        var settings = AppServices.Settings.Load();
        _gameFilter = settings.GameFilter;
        _autoUpdateEnabled = settings.AutoUpdateCheckEnabled;
        _ipsetFilter = AppServices.IpsetFilter.GetStatus(_ipsetAllPath);
        _theme = settings.Theme;
        _accentColorHex = settings.AccentColorHex;
        UpdateIpsetDescription();
        _loaded = true;
    }

    partial void OnGameFilterChanged(GameFilterMode value)
    {
        if (!_loaded) return;
        var settings = AppServices.Settings.Load();
        settings.GameFilter = value;
        AppServices.Settings.Save(settings);
        LastMessage = "Game Filter сохранён. Перезапустите стратегию, чтобы применить изменения.";
        LastError = null;
    }

    partial void OnAutoUpdateEnabledChanged(bool value)
    {
        if (!_loaded) return;
        var settings = AppServices.Settings.Load();
        settings.AutoUpdateCheckEnabled = value;
        AppServices.Settings.Save(settings);
    }

    [RelayCommand]
    private void CycleIpsetFilter()
    {
        try
        {
            var next = AppServices.IpsetFilter.GetNextMode(IpsetFilter);
            AppServices.IpsetFilter.SwitchTo(_ipsetAllPath, next);
            IpsetFilter = next;
            UpdateIpsetDescription();
            LastError = null;
            LastMessage = "Режим IPSet Filter изменён.";
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    [RelayCommand]
    private async Task UpdateIpsetListAsync()
    {
        IsBusy = true;
        LastError = null;
        try
        {
            await AppServices.ListUpdate.UpdateIpsetAllAsync(_ipsetAllPath);
            IpsetFilter = AppServices.IpsetFilter.GetStatus(_ipsetAllPath);
            UpdateIpsetDescription();
            LastMessage = "Список IPSet обновлён из репозитория.";
        }
        catch (Exception ex)
        {
            LastError = $"Не удалось обновить список: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenAdvanced() => AppServices.Modal.Show(new AdvancedModeWarningView());

    [RelayCommand]
    private void SetDarkTheme() => ApplyTheme(AppTheme.Dark);

    [RelayCommand]
    private void SetLightTheme() => ApplyTheme(AppTheme.Light);

    [RelayCommand]
    private void SetAccent(string hex)
    {
        AccentColorHex = hex;
        AppServices.Theme.ApplyAccent(hex);

        var settings = AppServices.Settings.Load();
        settings.AccentColorHex = hex;
        AppServices.Settings.Save(settings);
    }

    private void ApplyTheme(AppTheme theme)
    {
        Theme = theme;
        AppServices.Theme.ApplyTheme(theme);

        var settings = AppServices.Settings.Load();
        settings.Theme = theme;
        AppServices.Settings.Save(settings);
    }

    private void UpdateIpsetDescription()
    {
        IpsetFilterDescription = IpsetFilter switch
        {
            IpsetFilterMode.None => "none — ни один IP не попадает под фильтр",
            IpsetFilterMode.Loaded => "loaded — IP проверяется по списку ipset-all.txt",
            IpsetFilterMode.Any => "any — любой IP попадает под фильтр (влияет на многие сайты)",
            _ => IpsetFilter.ToString()
        };
    }
}
