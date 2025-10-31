using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Services.Interfaces;
using System; 

namespace Services.Implementations 
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _detailRepository;
        private readonly IOrderRepository _orderRepository; 

        public OrderDetailService()
        {
            _detailRepository = OrderDetailRepository.Instance;
            _orderRepository = OrderRepository.Instance;
        }

        public void AddOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (orderDetail.UnitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than zero.");

            if (orderDetail.OrderId <= 0)
                throw new ArgumentException("OrderId is required.");

            if (orderDetail.DishId <= 0)
                throw new ArgumentException("DishId is required.");

            _detailRepository.AddOrderDetail(orderDetail);

            // Tính lại tổng tiền
            UpdateOrderTotal(orderDetail.OrderId);
        }

        public void UpdateOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (orderDetail.UnitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than zero.");

            _detailRepository.UpdateOrderDetail(orderDetail);

            // Tính lại tổng tiền
            UpdateOrderTotal(orderDetail.OrderId);
        }

        public void DeleteOrderDetail(int id)
        {
            // Phải lấy OrderId TRƯỚC KHI XÓA
            var detail = _detailRepository.GetOrderDetailById(id);
            if (detail != null)
            {
                _detailRepository.DeleteOrderDetail(id);

                // Tính lại tổng tiền
                UpdateOrderTotal(detail.OrderId);
            }
        }

        public OrderDetail GetOrderDetailById(int id)
        {
            return _detailRepository.GetOrderDetailById(id);
        }

        public List<OrderDetail> GetOrderDetailsByOrderId(int orderId)
        {
            return _detailRepository.GetOrderDetailsByOrderId(orderId);
        }

        private void UpdateOrderTotal(int orderId)
        {
            // 1. Lấy tất cả chi tiết còn lại của đơn hàng
            var orderDetails = _detailRepository.GetOrderDetailsByOrderId(orderId);

            // 2. Tính tổng tiền mới
            decimal newTotalAmount = 0;
            if (orderDetails.Any())
            {
                // Dùng UnitPrice (giá lúc đặt) * Quantity
                newTotalAmount = orderDetails.Sum(d => d.UnitPrice * d.Quantity);
            }

            // 3. Cập nhật tổng tiền vào đơn hàng (Order) cha
            var orderToUpdate = _orderRepository.GetOrderById(orderId);
            if (orderToUpdate != null)
            {
                orderToUpdate.TotalAmount = newTotalAmount;
                _orderRepository.UpdateOrder(orderToUpdate);
            }
        }
    }
}