using System.Windows;
using System.Windows.Controls;
using ZapretGUI.App.Services;

namespace ZapretGUI.App.Views.Overlays;

public partial class AdvancedModeWarningView : UserControl
{
    public AdvancedModeWarningView()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Close();

    private void Continue_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new StrategiesPage());
}
