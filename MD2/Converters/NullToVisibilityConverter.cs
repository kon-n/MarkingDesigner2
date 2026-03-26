using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkingDesigner.Converters
{
    /// <summary>値が null のとき Collapsed、非 null のとき Visible。</summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
