using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication3.Models
{
    public partial class ProductList : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Product> _products;

        public ProductList()
        {
            _products = new ObservableCollection<Product>();
            // Adding some sample products
            Products.Add(new Product("Laptop", 999.99m, "Powerful laptop for work and gaming", 10));
            Products.Add(new Product("Smartphone", 499.99m, "Latest model with advanced features", 15));
            Products.Add(new Product("Headphones", 129.99m, "Noise canceling wireless headphones", 20));
        }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }

        public void RemoveProduct(Guid id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                Products.Remove(product);
            }
        }

        public void UpdateProduct(Product updatedProduct)
        {
            var product = Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product != null)
            {
                // Вместо замены объекта, обновляем его свойства
                // Это сохранит ссылку на объект и вызовет уведомления об изменении свойств
                product.Name = updatedProduct.Name;
                product.Price = updatedProduct.Price;
                product.Description = updatedProduct.Description;
                product.Quantity = updatedProduct.Quantity;
            }
        }
    }
} 