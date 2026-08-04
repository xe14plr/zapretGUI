using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ZapretGUI.Core.Models;

namespace ZapretGUI.App.Converters;

public sealed class NullOrEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Visibility.Visible;
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}

public sealed class SeverityToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DiagnosticSeverity severity
            ? severity switch
            {
                DiagnosticSeverity.Ok => Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24,
                DiagnosticSeverity.Warning => Wpf.Ui.Controls.SymbolRegular.Warning24,
                DiagnosticSeverity.Error => Wpf.Ui.Controls.SymbolRegular.ErrorCircle24,
                _ => Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24
            }
            : Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class UpToDateMultiConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasChecked = values.Length > 0 && values[0] is true;
        var updateAvailable = values.Length > 1 && values[1] is true;
        var hasError = values.Length > 2 && !string.IsNullOrEmpty(values[2] as string);
        return hasChecked && !updateAvailable && !hasError;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class SeverityToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush OkBrush = new(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly SolidColorBrush WarningBrush = new(Color.FromRgb(0xFF, 0xB3, 0x00));
    private static readonly SolidColorBrush ErrorBrush = new(Color.FromRgb(0xE5, 0x39, 0x35));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DiagnosticSeverity severity
            ? severity switch
            {
                DiagnosticSeverity.Ok => OkBrush,
                DiagnosticSeverity.Warning => WarningBrush,
                DiagnosticSeverity.Error => ErrorBrush,
                _ => OkBrush
            }
            : OkBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
