using Avalonia.Data.Converters;
using AvaloniaApplication3.Models;
using System;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class MessageTypeVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageType messageType && parameter is string stringParam)
            {
                var parsedType = Enum.Parse<MessageType>(stringParam, true);
                return messageType == parsedType;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 