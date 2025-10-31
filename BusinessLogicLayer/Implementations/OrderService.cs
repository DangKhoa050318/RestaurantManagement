using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System; 

namespace Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService()
        {
            _orderRepository = OrderRepository.Instance;
        }

        public List<Order> GetOrders() => _orderRepository.GetOrders();
        public Order GetOrderById(int id) => _orderRepository.GetOrderById(id);
        public List<Order> GetOrdersByCustomerId(int customerId) => _orderRepository.GetOrdersByCustomerId(customerId);

        public List<Order> GetOrdersByTableId(int tableId) => _orderRepository.GetOrdersByTableId(tableId);

        public void AddOrder(Order order)
        {
            if (order.TableId <= 0)
                throw new ArgumentException("TableId is required.");

            // CustomerId có thể null (khách vãng lai) nên không cần check <= 0

            if (string.IsNullOrWhiteSpace(order.Status))
                order.Status = "Pending"; // Trạng thái mặc định

            order.OrderTime = DateTime.Now;
            order.TotalAmount = 0; // Sẽ được cập nhật bởi OrderDetailService

            _orderRepository.AddOrder(order);
        }

        public void UpdateOrder(Order order)
        {
            var existing = _orderRepository.GetOrderById(order.OrderId);
            if (existing == null)
                throw new Exception("Order not found.");

            existing.TableId = order.TableId;
            existing.CustomerId = order.CustomerId;
            existing.Status = order.Status;
            existing.TotalAmount = order.TotalAmount;
            existing.OrderTime = order.OrderTime;

            _orderRepository.UpdateOrder(existing);
        }

        public void DeleteOrder(int id)
        {
            // Repository đã xử lý việc xóa các 'OrderDetail' con
            _orderRepository.DeleteOrder(id);
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            var validStatuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" }; 
            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Invalid status. Valid options: {string.Join(", ", validStatuses)}");

            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            order.Status = newStatus;
            _orderRepository.UpdateOrder(order);
        }
    }
}