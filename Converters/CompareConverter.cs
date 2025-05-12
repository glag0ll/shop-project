using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Avalonia.Data.Converters
{
    public class CompareConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IComparable comparable && parameter != null)
            {
                try
                {
                    var paramValue = System.Convert.ChangeType(parameter, value.GetType());
                    return comparable.CompareTo(paramValue) > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 