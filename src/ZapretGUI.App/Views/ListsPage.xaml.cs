using System.Windows.Controls;
using ZapretGUI.App.ViewModels;

namespace ZapretGUI.App.Views;

public partial class ListsPage : UserControl
{
    public ListsPage()
    {
        InitializeComponent();
        DataContext = new ListsViewModel();
    }
}
