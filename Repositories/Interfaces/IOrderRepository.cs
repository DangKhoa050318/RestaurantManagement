using BusinessObjects.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        List<Order> GetOrders();
        Order GetOrderById(int id);
        List<Order> GetOrdersByCustomerId(int customerId);
        List<Order> GetOrdersByTableId(int tableId); 
        
        /// <summary>
        /// Get orders within a date range (inclusive)
        /// </summary>
        List<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate);
        
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
    }
}