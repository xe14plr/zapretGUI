using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }
}
