using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class StrategiesPage : UserControl
{
    public StrategiesPage()
    {
        InitializeComponent();
        DataContext = new StrategiesViewModel();
    }
}
