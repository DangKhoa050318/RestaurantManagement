using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataAccessLayer.Repositories.Implementations
{
    public class AreaRepository : IAreaRepository
    {
        private static AreaRepository instance = null;
        private static readonly object instanceLock = new object();

        // TEST HOOK: allow tests to provide a DbContext factory that returns an in-memory context.
        // Default behavior keeps using parameterless constructor.
        public static Func<RestaurantMiniManagementDbContext> ContextFactory { get; set; } = () => new RestaurantMiniManagementDbContext();

        private AreaRepository() { }
        public static AreaRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AreaRepository();
                    }
                    return instance;
                }
            }
        }

        public List<Area> GetAreas()
        {
            using (var context = ContextFactory())
            {
                return context.Areas.Include(a => a.Tables).ToList();
            }
        }
        public Area GetAreaById(int areaId)
        {
            using (var context = ContextFactory())
            {
                return context.Areas
                              .Include(a => a.Tables)
                              .FirstOrDefault(a => a.AreaId == areaId);
            }
        }

        public void AddArea(Area area)
        {
            using (var context = ContextFactory())
            {
                // Track the entity
                var entry = context.Areas.Add(area);
                
                // Save to database - this should populate AreaId
                context.SaveChanges();
                
                // At this point, area.AreaId SHOULD have value from database
            }
        }

        public void UpdateArea(Area area)
        {
            using (var context = ContextFactory())
            {
                context.Areas.Update(area);
                context.SaveChanges();
            }
        }

        public void DeleteArea(int areaId)
        {
            using (var context = ContextFactory())
            {
                // Check if area has tables
                var tablesInArea = context.Tables.Where(t => t.AreaId == areaId).ToList();
                
                if (tablesInArea.Any())
                {
                    // Check if any table has orders
                    bool hasActiveOrders = tablesInArea.Any(table => 
                        context.Orders.Any(o => o.TableId == table.TableId));
                    
                    if (hasActiveOrders)
                    {
                        throw new Exception("Cannot delete area. Some tables have active orders. Please complete or cancel all orders first.");
                    }
                    
                    // No active orders - safe to delete tables
                    context.Tables.RemoveRange(tablesInArea);
                }

                // Now delete the area
                var area = context.Areas.Find(areaId);
                if (area != null)
                {
                    context.Areas.Remove(area);
                    context.SaveChanges();
                }
            }
        }
    }
}
