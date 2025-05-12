using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;
using Avalonia.Data;

namespace AvaloniaApplication3.Converters
{
    public class EmailValidationConverter : IValueConverter
    {
        // Регулярное выражение для проверки базовой структуры email
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Проверяем, не просят ли нас проверить наличие ошибки
            if (parameter is string paramString && paramString == "IsError")
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return false;

                string email = value.ToString() ?? string.Empty;
                
                // Проверка на пустое поле - не ошибка если пустой email (необязательное поле)
                if (string.IsNullOrWhiteSpace(email))
                    return false;
                    
                // Возвращаем true если есть ошибка
                if (!EmailRegex.IsMatch(email))
                    return true;
                    
                // Проверка на наличие точки в домене
                int atIndex = email.IndexOf('@');
                string domain = email.Substring(atIndex + 1);
                
                if (!domain.Contains("."))
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

            string email = value.ToString() ?? string.Empty;
            
            // Если email пустой, просто возвращаем его (валидация необязательного поля будет в ViewModel)
            if (string.IsNullOrWhiteSpace(email))
                return email;
                
            // Проверка что email содержит @ и структуру с доменом
            if (!EmailRegex.IsMatch(email))
            {
                return new BindingNotification(
                    new FormatException("Некорректный почтовый адрес. Требуется формат user@domain.com"),
                    BindingErrorType.Error);
            }
            
            // Проверка на наличие хотя бы одной точки в домене
            int atIndex = email.IndexOf('@');
            string domain = email.Substring(atIndex + 1);
            
            if (!domain.Contains("."))
            {
                return new BindingNotification(
                    new FormatException("Домен должен содержать хотя бы одну точку (например, domain.com)"),
                    BindingErrorType.Error);
            }
            
            return email;
        }
    }
} 