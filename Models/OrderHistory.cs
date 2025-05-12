using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaApplication3.Models
{
    public class OrderHistory
    {
        public ObservableCollection<Order> Orders { get; set; }

        public OrderHistory()
        {
            Orders = new ObservableCollection<Order>();
        }

        public void AddOrder(Order order)
        {
            Orders.Add(order);
        }

        public Order? GetOrderById(Guid id)
        {
            return Orders.FirstOrDefault(o => o.Id == id);
        }

        public Order? GetOrderByNumber(string orderNumber)
        {
            return Orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
        }

        public void UpdateOrderStatus(Guid orderId, string newStatus)
        {
            var order = GetOrderById(orderId);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                // Обновляем коллекцию для уведомления наблюдателей
                int index = Orders.IndexOf(order);
                if (index >= 0)
                {
                    Orders[index] = order;
                }
            }
        }
    }
} 