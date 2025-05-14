using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication3.Models
{
    public partial class BasketItem : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private Product _product = null!;

        [ObservableProperty]
        private int _quantity;

        // Вычисляемое свойство для общей стоимости позиции
        public decimal TotalPrice => Product?.Price * Quantity ?? 0;

        public BasketItem()
        {
            Id = Guid.NewGuid();
        }

        public BasketItem(Product product, int quantity)
        {
            Id = Guid.NewGuid();
            // Создаем копию продукта для корзины, не копируя его складское количество
            Product = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            };
            Quantity = quantity;
        }

        // Этот метод автоматически вызывается при изменении Quantity
        partial void OnQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
} 