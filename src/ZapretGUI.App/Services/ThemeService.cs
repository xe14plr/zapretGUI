using System.Windows;
using System.Windows.Media;
using ZapretGUI.Core.Models;

namespace ZapretGUI.App.Services;

/// <summary>
/// Swaps the app's Colors resource dictionary between Dark/Light and overrides the accent
/// color at runtime. Every color consumer in Styles/Controls.xaml uses DynamicResource, so
/// both operations propagate live without restarting the app.
/// </summary>
public sealed class ThemeService
{
    private static readonly Uri DarkUri = new("Styles/Colors.xaml", UriKind.Relative);
    private static readonly Uri LightUri = new("Styles/Colors.Light.xaml", UriKind.Relative);

    public static readonly string[] AccentSwatches =
    [
        "#8B5CF6", // violet (default)
        "#3B82F6", // blue
        "#22D3B5", // teal
        "#F472B6", // pink
        "#F59E0B", // amber
        "#22C55E", // green
        "#EF4444", // red
    ];

    public void Apply(AppTheme theme, string accentHex)
    {
        ApplyTheme(theme);
        ApplyAccent(accentHex);
    }

    public void ApplyTheme(AppTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var targetUri = theme == AppTheme.Light ? LightUri : DarkUri;

        for (var i = 0; i < dictionaries.Count; i++)
        {
            var source = dictionaries[i].Source?.OriginalString;
            if (source is not null && source.Contains("Colors", StringComparison.OrdinalIgnoreCase))
            {
                dictionaries[i] = new ResourceDictionary { Source = targetUri };
                return;
            }
        }
    }

    public void ApplyAccent(string hex)
    {
        Color color;
        try
        {
            color = (Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return;
        }

        var resources = Application.Current.Resources;
        resources["Color.Accent"] = color;
        resources["Brush.Accent"] = new SolidColorBrush(color);
        resources["Brush.Accent.Hover"] = new SolidColorBrush(Adjust(color, 0.14f));
        resources["Brush.Accent.Pressed"] = new SolidColorBrush(Adjust(color, -0.14f));
    }

    private static Color Adjust(Color color, float amount)
    {
        byte Shift(byte channel)
        {
            var delta = amount * 255;
            var value = channel + delta;
            return (byte)Math.Clamp(value, 0, 255);
        }

        return Color.FromRgb(Shift(color.R), Shift(color.G), Shift(color.B));
    }
}
