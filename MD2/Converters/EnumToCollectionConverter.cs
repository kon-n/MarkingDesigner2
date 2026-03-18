using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MarkingDesigner.Converters
{
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public Type? EnumType { get; set; }

        public EnumToCollectionConverter() { }
        public EnumToCollectionConverter(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var type = EnumType ?? value?.GetType();
            if (type != null && type.IsEnum)
            {
                return Enum.GetValues(type);
            }
            return Array.Empty<object>();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
