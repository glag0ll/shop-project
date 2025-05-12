using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;
using Avalonia.Data;

namespace AvaloniaApplication3.Converters
{
    public class PhoneValidationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Проверяем, не просят ли нас проверить наличие ошибки
            if (parameter is string paramString && paramString == "IsError")
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return false;

                string phone = value.ToString()?.Trim() ?? string.Empty;
                
                // Проверка на пустое поле - не показываем ошибку если пустой
                if (string.IsNullOrWhiteSpace(phone))
                    return false;
                    
                // Проверка на + в начале
                if (!phone.StartsWith("+"))
                    return true;
                
                // Проверка на недопустимые символы
                bool hasInvalidChars = false;
                foreach (char c in phone.Substring(1))
                {
                    if (!char.IsDigit(c) && c != ' ' && c != '(' && c != ')' && c != '-')
                    {
                        hasInvalidChars = true;
                        break;
                    }
                }
                
                if (hasInvalidChars)
                    return true;
                
                // Проверка на количество цифр
                int digitCount = phone.Count(char.IsDigit);
                if (digitCount != 11)
                    return true;
                
                return false;
            }
            
            // В обычном режиме просто возвращаем значение
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string phone = value.ToString()?.Trim() ?? string.Empty;
            
            // Если телефон пустой, просто возвращаем его (валидация обязательного поля будет в ViewModel)
            if (string.IsNullOrWhiteSpace(phone))
                return phone;
                
            // Проверка на наличие + в начале
            if (!phone.StartsWith("+"))
            {
                return new BindingNotification(
                    new FormatException("Номер телефона должен начинаться с +"),
                    BindingErrorType.Error);
            }
            
            // Проверка что в номере только цифры, + и возможные скобки/дефисы/пробелы
            bool hasInvalidChars = false;
            foreach (char c in phone.Substring(1))
            {
                if (!char.IsDigit(c) && c != ' ' && c != '(' && c != ')' && c != '-')
                {
                    hasInvalidChars = true;
                    break;
                }
            }
            
            if (hasInvalidChars)
            {
                return new BindingNotification(
                    new FormatException("Номер телефона содержит недопустимые символы"),
                    BindingErrorType.Error);
            }
            
            // Подсчитываем количество цифр в номере
            int digitCount = phone.Count(char.IsDigit);
            
            if (digitCount != 11)
            {
                return new BindingNotification(
                    new FormatException($"Номер телефона должен содержать 11 цифр, сейчас: {digitCount}"),
                    BindingErrorType.Error);
            }
            
            return phone;
        }
    }
} 