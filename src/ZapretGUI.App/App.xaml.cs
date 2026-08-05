using System.Windows;
using ZapretGUI.App.Services;

namespace ZapretGUI.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = AppServices.Settings.Load();
        AppServices.Theme.Apply(settings.Theme, settings.AccentColorHex);
    }
}
