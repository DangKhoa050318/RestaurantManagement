using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
            using (var context = new RestaurantDbContext())
            {
                context.Tables.Add(table);
                context.SaveChanges();
            }
        }

        public void DeleteTable(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                var table = context.Tables.Find(id);
                if (table != null)
                {
                    context.Tables.Remove(table);
                    context.SaveChanges();
                }
            }
        }

        public void UpdateTable(Table table)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Tables.Update(table);
                context.SaveChanges();
            }
        }

        public Table GetTableById(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Tables.Include(t => t.Area)
                                     .FirstOrDefault(t => t.TableId == id);
            }
        }

        public List<Table> GetTables()
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Tables.Include(t => t.Area).ToList();
            }
        }

    }
}