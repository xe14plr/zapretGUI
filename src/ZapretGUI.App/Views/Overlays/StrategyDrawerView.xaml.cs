using System.Windows.Controls;
using ZapretGUI.App.Services;

namespace ZapretGUI.App.Views.Overlays;

public partial class StrategyDrawerView : UserControl
{
    public StrategyDrawerView()
    {
        InitializeComponent();
    }

    private void StrategyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Binding SelectedItem to the already-selected strategy fires SelectionChanged once
        // during the initial sync (with RemovedItems empty, since the ListBox itself had
        // nothing selected yet) - only a genuine user pick has something in RemovedItems too.
        if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
        {
            AppServices.Modal.Close();
        }
    }
}
