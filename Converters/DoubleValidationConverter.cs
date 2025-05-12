using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Data;

namespace AvaloniaApplication3.Converters
{
    public class DoubleValidationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Просто пропускаем значение
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return 0.0;

            // Пытаемся преобразовать введенное значение в число
            if (double.TryParse(value.ToString(), NumberStyles.Any, culture, out double result))
            {
                // Проверка минимального значения
                if (parameter != null && parameter.ToString() == "NonNegative" && result < 0)
                {
                    return new BindingNotification(
                        new ArgumentOutOfRangeException("Значение не может быть отрицательным"),
                        BindingErrorType.Error);
                }
                
                return result;
            }
            else
            {
                // Если преобразование не удалось, возвращаем ошибку привязки
                return new BindingNotification(
                    new FormatException("Введено некорректное числовое значение"),
                    BindingErrorType.Error);
            }
        }
    }
} 