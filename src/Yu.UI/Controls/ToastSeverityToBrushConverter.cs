using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Yu.UI.Controls;

public sealed class ToastSeverityToBrushConverter : IValueConverter
{
    private static readonly Brush Info = new SolidColorBrush(Color.FromRgb(0x2D, 0x7D, 0xC9));
    private static readonly Brush Success = new SolidColorBrush(Color.FromRgb(0x2E, 0xB8, 0x72));
    private static readonly Brush Warning = new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23));
    private static readonly Brush Error = new SolidColorBrush(Color.FromRgb(0xE3, 0x4B, 0x4B));

    static ToastSeverityToBrushConverter()
    {
        Info.Freeze();
        Success.Freeze();
        Warning.Freeze();
        Error.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ToastSeverity severity) return Info;
        return severity switch
        {
            ToastSeverity.Success => Success,
            ToastSeverity.Warning => Warning,
            ToastSeverity.Error => Error,
            _ => Info
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
