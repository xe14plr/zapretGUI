using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGUI.App.Services;
using ZapretGUI.Core.Services;

namespace ZapretGUI.App.ViewModels;

public partial class UpdatesViewModel : ObservableObject
{
    public string LocalVersion => AppServices.BundledStrategiesVersion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateMessage))]
    private string? _latestVersion;

    [ObservableProperty]
    private bool _updateAvailable;

    public string UpdateMessage => $"Доступна новая версия: {LatestVersion}";

    [ObservableProperty]
    private bool _hasChecked;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    [RelayCommand]
    private async Task CheckAsync()
    {
        IsBusy = true;
        Error = null;
        try
        {
            var result = await AppServices.UpdateCheck.CheckAsync(LocalVersion);
            if (result.Error is not null)
            {
                Error = result.Error;
            }
            else
            {
                LatestVersion = result.LatestVersion;
                UpdateAvailable = result.UpdateAvailable;
            }

            HasChecked = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenReleasesPage()
    {
        Process.Start(new ProcessStartInfo(UpdateCheckService.ReleasesUrl) { UseShellExecute = true });
    }
}
