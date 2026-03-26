using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkingDesigner.Converters
{
    /// <summary>TextBox と 1〜99 の整数（V サイズ）を双方向変換。不正入力時は Binding.DoNothing でロールバック。</summary>
    public sealed class IntVClampConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i)
                return i.ToString(culture);
            return "4";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s)
                return Binding.DoNothing;
            s = s.Trim();
            if (s.Length == 0)
                return Binding.DoNothing;
            if (!int.TryParse(s, NumberStyles.Integer, culture, out int i))
                return Binding.DoNothing;
            return Math.Clamp(i, 1, 99);
        }
    }
}
