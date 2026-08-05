using System.Windows;
using System.Windows.Controls;

namespace ZapretGUI.App.Services;

public enum ModalPlacement
{
    Center,
    Bottom
}

/// <summary>
/// Drives the in-app overlay layer hosted in MainWindow: shows arbitrary content either as a
/// centered dialog or a bottom-anchored drawer, replacing the need for secondary Windows.
/// MainWindow calls Attach(...) once at startup; any ViewModel/View can call Show/Close after that.
/// </summary>
public sealed class ModalService
{
    private Grid? _overlayLayer;
    private Border? _panel;
    private ContentControl? _content;

    public bool IsOpen => _overlayLayer?.Visibility == Visibility.Visible;

    public void Attach(Grid overlayLayer, Border scrim, Border panel, ContentControl content)
    {
        _overlayLayer = overlayLayer;
        _panel = panel;
        _content = content;
        scrim.MouseLeftButtonDown += (_, _) => Close();
    }

    public void Show(FrameworkElement content, ModalPlacement placement = ModalPlacement.Center)
    {
        if (_overlayLayer is null || _panel is null || _content is null)
        {
            return;
        }

        _content.Content = content;

        if (placement == ModalPlacement.Bottom)
        {
            _panel.VerticalAlignment = VerticalAlignment.Bottom;
            _panel.MaxWidth = 640;
            _panel.MaxHeight = double.PositiveInfinity;
            _panel.Margin = new Thickness(24);
        }
        else
        {
            _panel.VerticalAlignment = VerticalAlignment.Center;
            _panel.MaxWidth = 640;
            _panel.MaxHeight = 620;
            _panel.Margin = new Thickness(24);
        }

        _overlayLayer.Visibility = Visibility.Visible;
    }

    public void Close()
    {
        if (_overlayLayer is null)
        {
            return;
        }

        _overlayLayer.Visibility = Visibility.Collapsed;

        if (_content is not null)
        {
            _content.Content = null;
        }
    }
}
