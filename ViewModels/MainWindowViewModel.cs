using System;
using System.Linq;
using System.Windows.Input;
using AvaloniaApplication3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AvaloniaApplication3.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ProductList _productList = null!;

        [ObservableProperty]
        private BasketList _basketList = null!;

        [ObservableProperty]
        private OrderHistory _orderHistory = null!;

        [ObservableProperty]
        private Product? _selectedProduct;

        [ObservableProperty]
        private BasketItem? _selectedBasketItem;

        [ObservableProperty]
        private Order? _selectedOrder;

        [ObservableProperty]
        private Order _currentOrder = null!;

        [ObservableProperty]
        private MessageInfo _currentMessage = null!;

        [ObservableProperty]
        private bool _showMessage;

        [ObservableProperty]
        private bool _showEditDialog;

        [ObservableProperty]
        private bool _showAddDialog;

        [ObservableProperty]
        private bool _showCheckoutDialog;

        [ObservableProperty]
        private bool _showOrderCompletedDialog;

        [ObservableProperty]
        private Product _editingProduct = null!;

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

        [RelayCommand]
        private void AddToBasket(Product product)
        {
            if (product == null)
            {
                ShowMessageBox("Не выбран товар для добавления в корзину", MessageType.Error);
                return;
            }

            if (product.Quantity <= 0)
            {
                ShowMessageBox($"Товар '{product.Name}' закончился на складе", MessageType.Warning);
                return;
            }

            // Уменьшаем количество товара на складе
            if (product.Quantity < 1)
            {
                ShowMessageBox($"Недостаточно товара '{product.Name}' на складе", MessageType.Warning);
                return;
            }
            
            // Уменьшаем количество товара на складе
            product.Quantity--;
            
            // Добавляем товар в корзину
            BasketList.AddToBasket(product, 1);
            // Обновляем общую стоимость (вызывается автоматически в BasketList.AddToBasket)

            // Обновляем товар в списке
            ProductList.UpdateProduct(product);
                     
            // Показать уведомление
            ShowMessageBox($"Товар '{product.Name}' добавлен в корзину. Осталось на складе: {product.Quantity}", MessageType.Success);
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
                 
            // Показать уведомление пользователю
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
             
            // Показать уведомление пользователю
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
            
            // Обновление общей стоимости корзины происходит автоматически через привязки
            
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

        public MainWindowViewModel()
        {
            // Initialize all objects to prevent null reference exceptions
            ProductList = new ProductList();
            BasketList = new BasketList();
            OrderHistory = new OrderHistory();
            EditingProduct = new Product();
            CurrentOrder = new Order();
            SelectedProduct = null;
            SelectedBasketItem = null;
            SelectedOrder = null;
            CurrentMessage = new MessageInfo("", MessageType.Info);
            ShowMessage = false;
            ShowEditDialog = false;
            ShowAddDialog = false;
            ShowCheckoutDialog = false;
            ShowOrderCompletedDialog = false;
            
            // Инициализация методов оплаты
            PaymentMethods = new ObservableCollection<string>(
                Enum.GetValues(typeof(PaymentMethod))
                    .Cast<PaymentMethod>()
                    .Select(p => p.ToString())
            );
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault() ?? string.Empty;
        }

        private void ShowMessageBox(string message, MessageType messageType = MessageType.Info, string title = "")
        {
            CurrentMessage = new MessageInfo(message, messageType, title);
            ShowMessage = true;
        }

        [RelayCommand]
        private void ShowProductEditDialog(Product product)
        {
            if (product == null)
            {
                ShowMessageBox("Не выбран товар для редактирования", MessageType.Error);
                return;
            }

            EditingProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Quantity = product.Quantity
            };
            ShowEditDialog = true;
        }

        [RelayCommand]
        private void ShowProductAddDialog()
        {
            EditingProduct = new Product
            {
                Name = "",
                Description = "",
                Price = 100.00m,
                Quantity = 1
            };
            ShowAddDialog = true;
        }

        [RelayCommand]
        private void SaveProduct()
        {
            try
            {
                // Проверка названия продукта
                if (string.IsNullOrWhiteSpace(EditingProduct.Name))
                {
                    ShowMessageBox("Название товара не может быть пустым. Пожалуйста, введите название товара.", MessageType.Error);
                    return;
                }

                // Проверка цены продукта
                if (EditingProduct.Price <= 0)
                {
                    ShowMessageBox("Цена товара должна быть больше нуля. Пожалуйста, введите корректную цену.", MessageType.Error);
                    return;
                }

                // Проверка количества продукта
                if (EditingProduct.Quantity < 0)
                {
                    ShowMessageBox("Количество товара не может быть отрицательным. Пожалуйста, введите корректное количество.", MessageType.Error);
                    return;
                }

                // Дополнительная проверка длины названия
                if (EditingProduct.Name.Length > 100)
                {
                    ShowMessageBox("Название товара слишком длинное. Максимальная длина - 100 символов.", MessageType.Error);
                    return;
                }

                // Дополнительная проверка длины описания
                if (EditingProduct.Description != null && EditingProduct.Description.Length > 1000)
                {
                    ShowMessageBox("Описание товара слишком длинное. Максимальная длина - 1000 символов.", MessageType.Error);
                    return;
                }

                // Проверка на слишком большую цену
                if (EditingProduct.Price > 1000000)
                {
                    ShowMessageBox("Указана слишком высокая цена. Максимальная цена - 1 000 000.", MessageType.Error);
                    return;
                }

                // Проверка на слишком большое количество
                if (EditingProduct.Quantity > 10000)
                {
                    ShowMessageBox("Указано слишком большое количество. Максимальное количество - 10 000.", MessageType.Error);
                    return;
                }

                if (ShowEditDialog)
                {
                    ProductList.UpdateProduct(EditingProduct);
                    ShowEditDialog = false;
                    
                    // Показать уведомление пользователю
                    ShowMessageBox($"Товар '{EditingProduct.Name}' успешно обновлен", MessageType.Success);
                }
                else if (ShowAddDialog)
                {
                    EditingProduct.Id = Guid.NewGuid();
                    ProductList.AddProduct(EditingProduct);
                    ShowAddDialog = false;
                    
                    // Показать уведомление пользователю
                    ShowMessageBox($"Товар '{EditingProduct.Name}' успешно добавлен", MessageType.Success);
                }
            }
            catch (FormatException ex)
            {
                ShowMessageBox($"Ошибка формата данных: {ex.Message}. Пожалуйста, проверьте правильность ввода числовых значений.", MessageType.Error);
            }
            catch (OverflowException ex)
            {
                ShowMessageBox($"Введенное значение слишком большое или слишком маленькое: {ex.Message}", MessageType.Error);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Произошла ошибка при сохранении товара: {ex.Message}", MessageType.Error);
            }
        }

        [RelayCommand]
        private void DeleteProduct(Product product)
        {
            if (product == null)
            {
                ShowMessageBox("Не выбран товар для удаления", MessageType.Error);
                return;
            }

            string productName = product.Name;
                 
            // Check if product is in basket
            var basketItem = BasketList.Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (basketItem != null)
            {
                ShowMessageBox($"Нельзя удалить товар '{productName}', который находится в корзине", MessageType.Warning);
                return;
            }

            ProductList.RemoveProduct(product.Id);
                 
            // Показать уведомление пользователю
            ShowMessageBox($"Товар '{productName}' успешно удален", MessageType.Success);
        }

        [RelayCommand]
        private void CloseMessageDialog()
        {
            ShowMessage = false;
        }

        [RelayCommand]
        private void CloseEditDialog()
        {
            ShowEditDialog = false;
            ShowAddDialog = false;
        }

        [RelayCommand]
        private void ShowCheckout()
        {
            if (BasketList.Items.Count == 0)
            {
                ShowMessageBox("Ваша корзина пуста. Пожалуйста, добавьте товары перед оформлением заказа.", MessageType.Warning, "Пустая корзина");
                return;
            }

            CurrentOrder = new Order(); // Создаем новый заказ
            CurrentOrder.AddItems(BasketList.Items); // Копируем товары из корзины
            // TotalAmount in CurrentOrder should update automatically due to changes in Order.cs

            // Сбрасываем ошибки валидации и выбранный метод оплаты
            NameError = string.Empty;
            EmailError = string.Empty;
            PhoneError = string.Empty;
            AddressError = string.Empty;
            PaymentError = string.Empty;
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault() ?? string.Empty;
            
            ShowCheckoutDialog = true;
        }
        
        [RelayCommand]
        private void CloseCheckoutDialog()
        {
            ShowCheckoutDialog = false;
        }
        
        [RelayCommand]
        private void CloseOrderCompletedDialog()
        {
            ShowOrderCompletedDialog = false;
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
            if (!ValidateOrderForm())
            {
                ShowMessageBox("Пожалуйста, исправьте ошибки в форме перед оформлением заказа.", MessageType.Error, "Ошибка валидации");
                return;
            }
            
            // CurrentOrder already contains all necessary data and TotalAmount
            CurrentOrder.OrderStatus = "Обрабатывается";
            CurrentOrder.PaymentMethod = SelectedPaymentMethod; // Set selected payment method

            OrderHistory.AddOrder(new Order
            {
                Id = CurrentOrder.Id,
                CustomerName = CurrentOrder.CustomerName,
                CustomerEmail = CurrentOrder.CustomerEmail,
                CustomerPhone = CurrentOrder.CustomerPhone,
                DeliveryAddress = CurrentOrder.DeliveryAddress,
                Items = new ObservableCollection<BasketItem>(CurrentOrder.Items.Select(i => new BasketItem(i.Product, i.Quantity))), // Deep copy
                OrderDate = CurrentOrder.OrderDate,
                PaymentMethod = CurrentOrder.PaymentMethod,
                OrderStatus = CurrentOrder.OrderStatus,
                OrderNumber = CurrentOrder.OrderNumber
            });

            ShowCheckoutDialog = false;
            ShowOrderCompletedDialog = true;
            BasketList.ClearBasket(); // Clear basket after successful order
        }
    }
}