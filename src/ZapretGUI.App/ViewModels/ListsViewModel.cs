using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using ZapretGUI.App.Services;
using ZapretGUI.Core.Services;

namespace ZapretGUI.App.ViewModels;

public partial class ListsViewModel : ObservableObject
{
    public ObservableCollection<ListFileTabViewModel> Tabs { get; } = new();

    [ObservableProperty]
    private ListFileTabViewModel? _selectedTab;

    public ListsViewModel()
    {
        UserListsService.EnsureDefaults(AppPaths.ListsDir);

        Tabs.Add(new ListFileTabViewModel(
            Path.Combine(AppPaths.ListsDir, "list-general-user.txt"),
            "Домены (свои)",
            "Дополнительные домены для обхода. Поддомены учитываются автоматически."));

        Tabs.Add(new ListFileTabViewModel(
            Path.Combine(AppPaths.ListsDir, "list-exclude-user.txt"),
            "Исключения доменов",
            "Домены, которые нужно исключить из обхода."));

        Tabs.Add(new ListFileTabViewModel(
            Path.Combine(AppPaths.ListsDir, "ipset-exclude-user.txt"),
            "Исключения IP",
            "IP-адреса и подсети, исключаемые из обхода."));

        Tabs.Add(new ListFileTabViewModel(
            Path.Combine(AppPaths.ListsDir, "ipset-all.txt"),
            "IPSet (основной список)",
            "Основной список IP/подсетей. Обычно обновляется кнопкой «Обновить список IPSet» на странице Настройки."));

        SelectedTab = Tabs.FirstOrDefault();
    }
}
