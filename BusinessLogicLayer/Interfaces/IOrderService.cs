using BusinessObjects.Models;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface IOrderService
    {
        List<Order> GetOrders();
        Order GetOrderById(int id);
        List<Order> GetOrdersByCustomerId(int customerId);
        List<Order> GetOrdersByTableId(int tableId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
        void UpdateOrderStatus(int orderId, string newStatus);
    }
}