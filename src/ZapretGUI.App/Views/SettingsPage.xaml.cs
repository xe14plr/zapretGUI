using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}
