using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories.Implementations
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private static OrderDetailRepository instance = null;
        private static readonly object instanceLock = new object();
        private OrderDetailRepository() { }
        public static OrderDetailRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new OrderDetailRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddOrderDetail(OrderDetail orderDetail)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.OrderDetails.Add(orderDetail);
                context.SaveChanges();
            }
        }

        public void DeleteOrderDetail(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                var detail = context.OrderDetails.Find(id);
                if (detail != null)
                {
                    context.OrderDetails.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public OrderDetail GetOrderDetailById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.OrderDetails
                              .Include(od => od.Dish)
                              .FirstOrDefault(od => od.OrderDetailId == id);
            }
        }

        public List<OrderDetail> GetOrderDetailsByOrderId(int orderId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.OrderDetails
                              .Include(od => od.Dish)
                              .Where(od => od.OrderId == orderId)
                              .ToList();
            }
        }

        public void UpdateOrderDetail(OrderDetail orderDetail)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.OrderDetails.Update(orderDetail);
                context.SaveChanges();
            }
        }
    }
}