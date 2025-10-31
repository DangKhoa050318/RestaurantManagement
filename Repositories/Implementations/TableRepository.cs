using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System; // Cần cho Exception

namespace DataAccessLayer.Repositories.Implementations
{
    public class TableRepository : ITableRepository
    {
        private static TableRepository instance = null;
        private static readonly object instanceLock = new object();
        private TableRepository() { }
        public static TableRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new TableRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddTable(Table table)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Tables.Add(table);
                context.SaveChanges();
            }
        }

        public void DeleteTable(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                // Không xóa bàn nếu đang có Order
                bool hasOrders = context.Orders.Any(o => o.TableId == id);
                if (hasOrders)
                {
                    throw new Exception("Cannot delete table. It has existing orders.");
                }

                var table = context.Tables.Find(id);
                if (table != null)
                {
                    context.Tables.Remove(table);
                    context.SaveChanges();
                }
            }
        }

        public Table GetTableById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Tables
                              .Include(t => t.Area)
                              .Include(t => t.Orders) 
                              .FirstOrDefault(t => t.TableId == id);
            }
        }

        public List<Table> GetTables()
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Tables
                              .Include(t => t.Area)
                              .Include(t => t.Orders)
                              .ToList();
            }
        }

        public void UpdateTable(Table table)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                // Tìm đối tượng đang được theo dõi (existing)
                var existing = context.Tables.Find(table.TableId);
                if (existing != null)
                {
                    // Chỉ cập nhật các giá trị, không đụng đến navigation properties
                    context.Entry(existing).CurrentValues.SetValues(table);
                    context.SaveChanges();
                }
            }
        }
    }
}