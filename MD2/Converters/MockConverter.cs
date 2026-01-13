using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkingDesigner.Converters
{
    public class MmToDipConverter : IValueConverter
    {
        private const double Dpi = 96.0;
        private const double MmPerInch = 25.4;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double mm)
            {
                return (mm / MmPerInch) * Dpi;
            }
            if (value is int mmInt)
            {
                return ((double)mmInt / MmPerInch) * Dpi;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dip)
            {
                return (dip * MmPerInch) / Dpi;
            }
            return 0.0;
        }
    }
}