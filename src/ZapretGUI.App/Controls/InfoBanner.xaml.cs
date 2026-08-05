using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZapretGUI.App.Controls;

public partial class InfoBanner : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(InfoBanner), new PropertyMetadata(null, OnAnyChanged));

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(InfoBanner), new PropertyMetadata(null, OnAnyChanged));

    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
        nameof(Severity), typeof(string), typeof(InfoBanner), new PropertyMetadata("Info", OnAnyChanged));

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(InfoBanner), new PropertyMetadata(false, OnAnyChanged));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Severity
    {
        get => (string)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public InfoBanner()
    {
        InitializeComponent();
        Refresh();
    }

    private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((InfoBanner)d).Refresh();

    private void Refresh()
    {
        Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;

        TitleText.Text = Title;
        TitleText.Visibility = string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;

        MessageText.Text = Message;
        MessageText.Visibility = string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;

        var (colorKey, bgKey, iconKey) = Severity switch
        {
            "Success" => ("Color.Success", "Brush.Success.Bg", "Icon.CheckCircle"),
            "Warning" => ("Color.Warning", "Brush.Warning.Bg", "Icon.Warning"),
            "Error" => ("Color.Error", "Brush.Error.Bg", "Icon.ErrorCircle"),
            _ => ("Color.Info", "Brush.Info.Bg", "Icon.Info")
        };

        if (TryFindResource(colorKey) is Color color)
        {
            Root_Border.BorderBrush = new SolidColorBrush(color) { Opacity = 0.5 };
            IconPath.Stroke = new SolidColorBrush(color);
            IconPath.Fill = Brushes.Transparent;
        }

        if (TryFindResource(bgKey) is Brush bg)
        {
            Root_Border.Background = bg;
        }

        if (TryFindResource(iconKey) is Geometry geometry)
        {
            IconPath.Data = geometry;
            IconPath.StrokeThickness = 1.6;
        }
    }
}
