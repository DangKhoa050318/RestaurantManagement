using BusinessObjects.Models;
using System.Collections.Generic;
namespace BusinessLogicLayer.Services.Interfaces
{
    public interface IOrderService
    {
        List<Order> GetOrders();
        Order GetOrderById(int id);
        List<Order> GetOrdersByCustomerId(int customerId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
    }
}