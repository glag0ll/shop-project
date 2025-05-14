using System;
using System.Linq;
using System.Collections.ObjectModel;
using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaApplication3.ViewModels
{
    public partial class BasketWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private BasketList _basketList = null!;

        [ObservableProperty]
        private ProductList _productList = null!;

        [ObservableProperty]
        private BasketItem? _selectedBasketItem;

        [ObservableProperty]
        private MessageInfo _currentMessage;

        [ObservableProperty]
        private bool _showMessage;

        [ObservableProperty]
        private bool _showCheckoutDialog;

        [ObservableProperty]
        private bool _testDialogVisible = false;

        [ObservableProperty]
        private bool _showOrderCompletedDialog;

        [ObservableProperty]
        private Order _currentOrder = null!;

        [ObservableProperty]
        private ObservableCollection<string> _paymentMethods = null!;

        [ObservableProperty]
        private string _selectedPaymentMethod = string.Empty;

        [ObservableProperty]
        private string _emailError = string.Empty;

        [ObservableProperty]
        private string _phoneError = string.Empty;

        [ObservableProperty]
        private string _nameError = string.Empty;

        [ObservableProperty]
        private string _addressError = string.Empty;

        [ObservableProperty]
        private string _paymentError = string.Empty;

        // Event to notify when to navigate back to the main window
        public event EventHandler? NavigateBack;
        
        // We need a reference to the MainWindowViewModel to ensure data consistency
        private readonly MainWindowViewModel _mainViewModel;
        
        public BasketWindowViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            BasketList = mainViewModel.BasketList;
            ProductList = mainViewModel.ProductList;
            
            // Инициализируем объект сообщения
            _currentMessage = new MessageInfo("", MessageType.Info);
            _showMessage = false;
            
            // Инициализация для формы оформления заказа
            CurrentOrder = new Order();
            
            // Инициализация методов оплаты
            PaymentMethods = new ObservableCollection<string>(
                Enum.GetValues(typeof(PaymentMethod))
                    .Cast<PaymentMethod>()
                    .Select(p => p.ToString())
            );
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault() ?? string.Empty;
        }
        
        [RelayCommand]
        private void RemoveFromBasket(BasketItem basketItem)
        {
            if (basketItem == null)
            {
                ShowMessageBox("Не выбран товар для удаления из корзины", MessageType.Error);
                return;
            }

            string productName = basketItem.Product.Name;
            
            // Возвращаем количество товара обратно на склад
            var product = ProductList.Products.FirstOrDefault(p => p.Id == basketItem.Product.Id);
            if (product != null)
            {
                // Увеличиваем количество товара на складе
                product.Quantity += basketItem.Quantity;
                ProductList.UpdateProduct(product);
            }

            BasketList.RemoveFromBasket(basketItem.Id);
                
            // Показать уведомление пользователю в окне корзины
            ShowMessageBox($"Товар '{productName}' удален из корзины", MessageType.Success);
        }

        [RelayCommand]
        private void ClearBasket()
        {
            if (BasketList.Items.Count == 0)
            {
                ShowMessageBox("Корзина уже пуста", MessageType.Info);
                return;
            }
             
            // Возвращаем все товары обратно на склад
            foreach (var item in BasketList.Items)
            {
                var product = ProductList.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                if (product != null)
                {
                    // Увеличиваем количество товара на складе
                    product.Quantity += item.Quantity;
                    ProductList.UpdateProduct(product);
                }
            }

            BasketList.ClearBasket();
             
            // Показать уведомление пользователю в окне корзины
            ShowMessageBox("Корзина успешно очищена", MessageType.Success);
        }
        
        // Общий метод для изменения количества товара в корзине
        private void ChangeBasketItemQuantity(BasketItem basketItem, int change)
        {
            if (basketItem == null)
            {
                ShowMessageBox("Не выбран товар для изменения количества", MessageType.Error);
                return;
            }

            // Находим товар на складе
            var product = ProductList.Products.FirstOrDefault(p => p.Id == basketItem.Product.Id);
            if (product == null)
            {
                ShowMessageBox("Товар не найден на складе", MessageType.Error);
                return;
            }

            // Проверяем особые случаи в зависимости от изменения количества
            if (change > 0)
            {
                // Увеличение - проверяем наличие на складе
                if (product.Quantity < change)
                {
                    ShowMessageBox($"Недостаточно товара на складе. Доступно: {product.Quantity}", MessageType.Warning);
                    return;
                }

                // Уменьшаем количество на складе
                product.Quantity -= change;
                
                // Увеличиваем количество в корзине
                basketItem.Quantity += change;
            }
            else if (change < 0)
            {
                // Уменьшение - проверяем минимальное количество
                if (basketItem.Quantity <= Math.Abs(change))
                {
                    // Если количество станет 0 или меньше, удаляем товар из корзины
                    RemoveFromBasket(basketItem);
                    return;
                }

                // Уменьшаем количество в корзине и возвращаем на склад
                basketItem.Quantity += change; // change отрицательный
                product.Quantity -= change;    // двойной минус даст плюс
            }
            else
            {
                // Нет изменений
                return;
            }

            // Обновляем товар на списке
            ProductList.UpdateProduct(product);
            
            // Формируем сообщение
            string action = change > 0 ? "увеличено" : "уменьшено";
            string debugInfo = $"В корзине: {basketItem.Quantity} шт. Осталось на складе: {product.Quantity} шт.";
            ShowMessageBox($"Количество товара '{basketItem.Product.Name}' {action}. {debugInfo}", MessageType.Success);
        }
        
        [RelayCommand]
        public void IncreaseBasketItemQuantity(BasketItem basketItem)
        {
            ChangeBasketItemQuantity(basketItem, 1);
        }
        
        [RelayCommand]
        public void DecreaseBasketItemQuantity(BasketItem basketItem)
        {
            ChangeBasketItemQuantity(basketItem, -1);
        }
        
        [RelayCommand]
        private void Checkout()
        {
            if (BasketList == null)
            {
                ShowMessageBox("Ошибка: BasketList не инициализирован", MessageType.Error);
                return;
            }
            
            if (BasketList.Items.Count == 0)
            {
                ShowMessageBox("Ваша корзина пуста. Пожалуйста, добавьте товары перед оформлением заказа.", MessageType.Warning, "Пустая корзина");
                return;
            }

            try
            {
                // Создаем новый заказ
                CurrentOrder = new Order(); 
                
                // Копируем товары из корзины и проверяем, что они успешно добавлены
                CurrentOrder.AddItems(BasketList.Items);
                
                if (CurrentOrder.Items.Count == 0)
                {
                    ShowMessageBox("Ошибка при добавлении товаров в заказ", MessageType.Error);
                    return;
                }

                // Сбрасываем ошибки валидации и выбранный метод оплаты
                NameError = string.Empty;
                EmailError = string.Empty;
                PhoneError = string.Empty;
                AddressError = string.Empty;
                PaymentError = string.Empty;
                
                // Устанавливаем метод оплаты по умолчанию
                SelectedPaymentMethod = PaymentMethods.FirstOrDefault() ?? string.Empty;
                
                // Показываем диалог оформления заказа
                ShowCheckoutDialog = true;
                TestDialogVisible = true;
                
                // Явный вызов уведомлений об изменении свойств
                OnPropertyChanged(nameof(ShowCheckoutDialog));
                OnPropertyChanged(nameof(TestDialogVisible));
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Произошла ошибка при оформлении заказа: {ex.Message}", MessageType.Error);
            }
        }
        
        [RelayCommand]
        private void CloseCheckoutDialog()
        {
            ShowCheckoutDialog = false;
            TestDialogVisible = false;
        }
        
        [RelayCommand]
        private void CloseOrderCompletedDialog()
        {
            ShowOrderCompletedDialog = false;
            
            // После завершения заказа возвращаемся в основное окно
            NavigateBack?.Invoke(this, EventArgs.Empty);
        }
        
        [RelayCommand]
        private void ValidateEmail()
        {
            ValidateEmailField(CurrentOrder.CustomerEmail);
        }

        [RelayCommand]
        private void ValidatePhone()
        {
            ValidatePhoneField(CurrentOrder.CustomerPhone);
        }

        [RelayCommand] 
        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(CurrentOrder.CustomerName))
            {
                NameError = "ФИО получателя не может быть пустым";
            }
            else
            {
                NameError = string.Empty;
            }
        }

        [RelayCommand]
        private void ValidateAddress()
        {
            if (string.IsNullOrWhiteSpace(CurrentOrder.DeliveryAddress))
            {
                AddressError = "Адрес доставки не может быть пустым";
            }
            else
            {
                AddressError = string.Empty;
            }
        }

        [RelayCommand]
        private void ValidatePaymentMethod()
        {
            if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
            {
                PaymentError = "Необходимо выбрать способ оплаты";
            }
            else
            {
                PaymentError = string.Empty;
            }
        }

        // Валидация почты с возвратом булева значения
        private bool ValidateEmailField(string email)
        {
            // Если поле пустое, не считаем ошибкой (необязательное поле)
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailError = string.Empty;
                return true;
            }

            // Проверка базового формата email
            if (!email.Contains("@"))
            {
                EmailError = "Введите корректный email в формате user@domain.com";
                return false;
            }

            // Проверка домена
            int atIndex = email.IndexOf('@');
            if (atIndex >= 0 && atIndex < email.Length - 1)
            {
                string domain = email.Substring(atIndex + 1);
                if (!domain.Contains("."))
                {
                    EmailError = "Домен должен содержать точку (например, domain.com)";
                    return false;
                }
            }

            EmailError = string.Empty;
            return true;
        }

        // Валидация телефона с возвратом булева значения
        private bool ValidatePhoneField(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                PhoneError = "Номер телефона не может быть пустым";
                return false;
            }

            // Проверка на + в начале
            if (!phone.StartsWith("+"))
            {
                PhoneError = "Номер телефона должен начинаться с +";
                return false;
            }

            // Проверка на недопустимые символы
            foreach (char c in phone.Substring(1))
            {
                if (!char.IsDigit(c) && c != ' ' && c != '(' && c != ')' && c != '-')
                {
                    PhoneError = "Номер телефона содержит недопустимые символы";
                    return false;
                }
            }

            // Подсчет цифр
            int digitCount = phone.Count(char.IsDigit);
            if (digitCount != 11)
            {
                PhoneError = $"Номер телефона должен содержать 11 цифр (сейчас: {digitCount})";
                return false;
            }

            PhoneError = string.Empty;
            return true;
        }

        // Метод для валидации всей формы заказа
        private bool ValidateOrderForm()
        {
            bool isValid = true;

            // Проверка имени
            if (string.IsNullOrWhiteSpace(CurrentOrder.CustomerName))
            {
                NameError = "ФИО получателя не может быть пустым";
                isValid = false;
            }
            else
            {
                NameError = string.Empty;
            }

            // Проверка телефона
            if (!ValidatePhoneField(CurrentOrder.CustomerPhone))
            {
                isValid = false;
            }

            // Проверка email (только если он указан)
            if (!string.IsNullOrWhiteSpace(CurrentOrder.CustomerEmail) && !ValidateEmailField(CurrentOrder.CustomerEmail))
            {
                isValid = false;
            }

            // Проверка адреса
            if (string.IsNullOrWhiteSpace(CurrentOrder.DeliveryAddress))
            {
                AddressError = "Адрес доставки не может быть пустым";
                isValid = false;
            }
            else
            {
                AddressError = string.Empty;
            }

            // Проверка способа оплаты
            if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
            {
                PaymentError = "Необходимо выбрать способ оплаты";
                isValid = false;
            }
            else
            {
                PaymentError = string.Empty;
            }

            return isValid;
        }

        [RelayCommand]
        private void CompleteOrder()
        {
            // Отладочный вывод
            Console.WriteLine("CompleteOrder method called");
            
            if (!ValidateOrderForm())
            {
                ShowMessageBox("Пожалуйста, исправьте ошибки в форме перед оформлением заказа.", MessageType.Error, "Ошибка валидации");
                return;
            }
            
            // Текущий заказ уже содержит все необходимые данные
            CurrentOrder.OrderStatus = "Обрабатывается";
            CurrentOrder.PaymentMethod = SelectedPaymentMethod; // Устанавливаем выбранный метод оплаты

            // Добавляем заказ в историю заказов через MainViewModel
            _mainViewModel.OrderHistory.AddOrder(new Order
            {
                Id = CurrentOrder.Id,
                CustomerName = CurrentOrder.CustomerName,
                CustomerEmail = CurrentOrder.CustomerEmail,
                CustomerPhone = CurrentOrder.CustomerPhone,
                DeliveryAddress = CurrentOrder.DeliveryAddress,
                Items = new ObservableCollection<BasketItem>(CurrentOrder.Items.Select(i => new BasketItem(i.Product, i.Quantity))), // Глубокая копия
                OrderDate = CurrentOrder.OrderDate,
                PaymentMethod = CurrentOrder.PaymentMethod,
                OrderStatus = CurrentOrder.OrderStatus,
                OrderNumber = CurrentOrder.OrderNumber
            });

            ShowCheckoutDialog = false;
            TestDialogVisible = false;
            ShowOrderCompletedDialog = true;
            BasketList.ClearBasket(); // Очищаем корзину после успешного заказа
        }
        
        [RelayCommand]
        private void GoBack()
        {
            // Notify that we want to go back to the main window
            NavigateBack?.Invoke(this, EventArgs.Empty);
        }
        
        private void ShowMessageBox(string message, MessageType messageType = MessageType.Info, string title = "")
        {
            // Создаем новый объект MessageInfo
            var newMessage = new MessageInfo(message, messageType, title);
            CurrentMessage = newMessage;
            
            // Включаем отображение сообщения
            ShowMessage = true;
        }
        
        [RelayCommand]
        private void CloseMessageDialog()
        {
            ShowMessage = false;
        }
    }
} 