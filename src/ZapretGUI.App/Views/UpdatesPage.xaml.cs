using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class UpdatesPage : Page
{
    public UpdatesPage()
    {
        InitializeComponent();
        DataContext = new UpdatesViewModel();
    }
}
