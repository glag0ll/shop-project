using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return enumValue.ToString().ToLower();
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 