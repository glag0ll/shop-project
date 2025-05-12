using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaApplication3.Models;
using System;
using System.Globalization;

namespace AvaloniaApplication3.Converters
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageType messageType)
            {
                return messageType switch
                {
                    MessageType.Success => new SolidColorBrush(Colors.Green),
                    MessageType.Error => new SolidColorBrush(Colors.Red),
                    MessageType.Warning => new SolidColorBrush(Colors.Orange),
                    MessageType.Info => new SolidColorBrush(Colors.DodgerBlue),
                    _ => new SolidColorBrush(Colors.DodgerBlue)
                };
            }
            return new SolidColorBrush(Colors.DodgerBlue);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 