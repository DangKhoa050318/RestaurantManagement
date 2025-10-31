using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

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
            using (var context = new RestaurantMiniManagementDbContext())
            {
                if (order.OrderTime == default)
                    order.OrderTime = DateTime.Now;

                context.Orders.Add(order);
                context.SaveChanges();
            }
        }

        public void DeleteOrder(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                var order = context.Orders.Include(o => o.OrderDetails)
                                         .FirstOrDefault(o => o.OrderId == id);
                if (order != null)
                {
                    context.OrderDetails.RemoveRange(order.OrderDetails);
                    context.Orders.Remove(order);
                    context.SaveChanges();
                }
            }
        }

        public Order GetOrderById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Dish) 
                    .FirstOrDefault(o => o.OrderId == id);
            }
        }

        public List<Order> GetOrders()
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Dish)
                    .ToList();
            }
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .Include(o => o.Table)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Dish)
                    .ToList();
            }
        }

        public List<Order> GetOrdersByTableId(int tableId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Orders
                    .Where(o => o.TableId == tableId)
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Dish)
                    .ToList();
            }
        }

        public void UpdateOrder(Order order)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Orders.Update(order);
                context.SaveChanges();
            }
        }
    }
}

