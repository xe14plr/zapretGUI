using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZapretGUI.App.ViewModels;

public partial class ListFileTabViewModel : ObservableObject
{
    private bool _suppressDirty;

    public string FilePath { get; }
    public string Title { get; }
    public string Description { get; }

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string? _statusMessage;

    public ListFileTabViewModel(string filePath, string title, string description)
    {
        FilePath = filePath;
        Title = title;
        Description = description;
        Reload();
    }

    partial void OnContentChanged(string value)
    {
        if (!_suppressDirty)
        {
            IsDirty = true;
        }
    }

    [RelayCommand]
    private void Reload()
    {
        _suppressDirty = true;
        Content = File.Exists(FilePath) ? File.ReadAllText(FilePath) : string.Empty;
        _suppressDirty = false;
        IsDirty = false;
        StatusMessage = null;
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, Content);
            IsDirty = false;
            StatusMessage = "Сохранено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }
}
