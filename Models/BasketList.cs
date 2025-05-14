using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication3.Models
{
    public partial class BasketList : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BasketItem> _items;

        public decimal TotalCost => Items?.Sum(item => item.TotalPrice) ?? 0;
        
        // Публичный метод для обновления TotalCost
        public void UpdateTotalCost()
        {
            OnPropertyChanged(nameof(TotalCost));
        }

        public BasketList()
        {
            _items = new ObservableCollection<BasketItem>();
            
            // Подписываемся на изменения коллекции
            _items.CollectionChanged += (s, e) => {
                // Всегда обновляем TotalCost при любых изменениях коллекции
                OnPropertyChanged(nameof(TotalCost));
                
                // Подписываемся на новые элементы
                if (e.NewItems != null)
                {
                    foreach (BasketItem item in e.NewItems)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
                
                // Отписываемся от удаленных элементов
                if (e.OldItems != null)
                {
                    foreach (BasketItem item in e.OldItems)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                }
            };
        }
        
        // Единый обработчик изменений в элементах корзины
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BasketItem.Quantity) || e.PropertyName == nameof(BasketItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalCost));
            }
        }

        // Базовые операции с корзиной
        public void AddToBasket(Product product, int quantity)
        {
            var existingItem = Items.FirstOrDefault(item => item.Product.Id == product.Id);
            
            if (existingItem != null)
            {
                // Если товар уже в корзине, увеличиваем количество
                existingItem.Quantity += quantity;
                // TotalCost обновится автоматически через PropertyChanged
            }
            else
            {
                // Добавляем новый товар
                Items.Add(new BasketItem(product, quantity));
                // TotalCost обновится автоматически через CollectionChanged
            }
        }

        public void RemoveFromBasket(Guid basketItemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == basketItemId);
            if (item != null)
            {
                Items.Remove(item);
                // TotalCost обновится автоматически через CollectionChanged
            }
        }

        public void UpdateQuantity(Guid basketItemId, int newQuantity)
        {
            var item = Items.FirstOrDefault(i => i.Id == basketItemId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    Items.Remove(item);
                    // TotalCost обновится автоматически через CollectionChanged
                }
                else
                {
                    item.Quantity = newQuantity;
                    // TotalCost обновится автоматически через PropertyChanged
                }
            }
        }

        public void ClearBasket()
        {
            Items.Clear();
            // TotalCost обновится автоматически через CollectionChanged
        }
    }
} 