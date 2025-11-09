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
        
        /// <summary>
        /// Get orders within a date range
        /// </summary>
        public List<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before or equal to end date.");

            return _orderRepository.GetOrdersByDateRange(startDate, endDate);
        }

        public void AddOrder(Order order)
        {
            if (order.TableId <= 0)
                throw new ArgumentException("TableId is required.");

            // CustomerId có thể null (khách vãng lai) nên không cần check <= 0

            if (string.IsNullOrWhiteSpace(order.Status))
                order.Status = "Scheduled"; // Trạng thái mặc định

            order.OrderTime = DateTime.Now;
            order.PaymentTime = null; // Chưa thanh toán
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
            existing.PaymentTime = order.PaymentTime; // ✅ Cập nhật PaymentTime

            _orderRepository.UpdateOrder(existing);
        }

        public void DeleteOrder(int id)
        {
            // Repository đã xử lý việc xóa các 'OrderDetail' con
            _orderRepository.DeleteOrder(id);
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" }; 
            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Invalid status. Valid options: {string.Join(", ", validStatuses)}");

            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            order.Status = newStatus;
            
            // ✅ Tự động set PaymentTime khi order Completed
            if (newStatus == "Completed" && order.PaymentTime == null)
            {
                order.PaymentTime = DateTime.Now;
            }
            
            _orderRepository.UpdateOrder(order);
        }

        /// <summary>
        /// Thanh toán đơn hàng - Set PaymentTime và Status = Completed
        /// </summary>
        public void PayOrder(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            if (order.Status == "Completed")
                throw new Exception("Order already paid.");

            if (order.Status == "Cancelled")
                throw new Exception("Cannot pay cancelled order.");

            order.PaymentTime = DateTime.Now;
            order.Status = "Completed";
            
            _orderRepository.UpdateOrder(order);
        }
    }
}