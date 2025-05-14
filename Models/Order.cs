using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication3.Models
{
    public partial class Order : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _customerName = string.Empty;

        [ObservableProperty]
        private string _customerEmail = string.Empty;

        [ObservableProperty]
        private string _customerPhone = string.Empty;

        [ObservableProperty]
        private string _deliveryAddress = string.Empty;

        private ObservableCollection<BasketItem> _items = new ObservableCollection<BasketItem>();
        public ObservableCollection<BasketItem> Items
        {
            get => _items;
            set
            {
                if (SetProperty(ref _items, value))
                {
                    OnPropertyChanged(nameof(TotalAmount));
                    if (_items != null)
                    {
                        _items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalAmount));
                    }
                }
            }
        }

        public decimal TotalAmount => Items?.Sum(item => item.TotalPrice) ?? 0;

        [ObservableProperty]
        private DateTime _orderDate;

        [ObservableProperty]
        private string _paymentMethod = string.Empty;

        [ObservableProperty]
        private string _orderStatus = "Новый";

        [ObservableProperty]
        private string _orderNumber = string.Empty;

        public Order()
        {
            Id = Guid.NewGuid();
            OrderDate = DateTime.Now;
            GenerateOrderNumber();
            
            if (_items == null)
            {
                _items = new ObservableCollection<BasketItem>();
            }
            
            _items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalAmount));
        }

        private void GenerateOrderNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = new Random().Next(1000, 9999).ToString();
            OrderNumber = $"ORDER-{datePart}-{randomPart}";
        }

        public void AddItems(IEnumerable<BasketItem> items)
        {
            if (items == null)
            {
                return;
            }
            
            if (_items == null)
            {
                _items = new ObservableCollection<BasketItem>();
                _items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalAmount));
            }
            
            _items.Clear();
            
            foreach (var item in items)
            {
                if (item != null && item.Product != null)
                {
                    _items.Add(new BasketItem(item.Product, item.Quantity));
                }
            }
            
            OnPropertyChanged(nameof(TotalAmount));
        }

        public void ClearItems()
        {
            Items.Clear();
        }
    }
} 