using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private static OrderRepository instance = null;
        private static readonly object instanceLock = new object();
        private OrderRepository() { }
        public static OrderRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new OrderRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddOrder(Order order)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Orders.Add(order);
                context.SaveChanges();
            }
        }

        public void DeleteOrder(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                var order = context.Orders.Include(o => o.OrderDetails)
                                        .FirstOrDefault(o => o.OrderId == id);
                if (order != null)
                {
                    // Xóa các chi tiết trước
                    context.OrderDetails.RemoveRange(order.OrderDetails);
                    // Sau đó xóa đơn hàng
                    context.Orders.Remove(order);
                    context.SaveChanges();
                }
            }
        }

        public Order GetOrderById(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food) // Lấy cả thông tin Food
                    .FirstOrDefault(o => o.OrderId == id);
            }
        }

        public List<Order> GetOrders()
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food)
                    .ToList();
            }
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food)
                    .ToList();
            }
        }

        public void UpdateOrder(Order order)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Orders.Update(order);
                context.SaveChanges();
            }
        }
    }
}

