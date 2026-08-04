using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }
}
