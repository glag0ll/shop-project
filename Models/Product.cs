using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication3.Models
{
    public partial class Product : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private decimal _price;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _quantity;

        // Проверка наличия товара на складе
        public bool IsAvailable => Quantity > 0;

        public Product()
        {
            Id = Guid.NewGuid();
        }

        public Product(string name, decimal price, string description, int quantity)
        {
            Id = Guid.NewGuid();
            Name = name;
            Price = price;
            Description = description;
            Quantity = quantity;
        }

        // Вспомогательные методы для работы с товаром
        
        // Уменьшает количество товара на складе
        public bool DecreaseQuantity(int amount = 1)
        {
            if (Quantity < amount)
                return false;
                
            Quantity -= amount;
            OnPropertyChanged(nameof(IsAvailable));
            return true;
        }
        
        // Увеличивает количество товара на складе
        public void IncreaseQuantity(int amount = 1)
        {
            Quantity += amount;
            OnPropertyChanged(nameof(IsAvailable));
        }
        
        // Создает копию продукта
        public Product Clone()
        {
            return new Product
            {
                Id = Id,
                Name = Name,
                Price = Price,
                Description = Description,
                Quantity = Quantity
            };
        }
    }
} 