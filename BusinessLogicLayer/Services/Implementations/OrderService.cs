using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;

namespace BusinessLogicLayer.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService()
        {
            _orderRepository = OrderRepository.Instance;
        }

        public void AddOrder(Order order) => _orderRepository.AddOrder(order);
        public void DeleteOrder(int id) => _orderRepository.DeleteOrder(id);
        public Order GetOrderById(int id) => _orderRepository.GetOrderById(id);
        public List<Order> GetOrders() => _orderRepository.GetOrders();
        public List<Order> GetOrdersByCustomerId(int customerId) => _orderRepository.GetOrdersByCustomerId(customerId);
        public void UpdateOrder(Order order) => _orderRepository.UpdateOrder(order);
    }
}