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
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Areas.Include(a => a.Tables).ToList();
            }
        }
        public Area GetAreaById(int areaId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Areas
                              .Include(a => a.Tables)
                              .FirstOrDefault(a => a.AreaId == areaId);
            }
        }

        public void AddArea(Area area)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Areas.Add(area);
                context.SaveChanges();
            }
        }

        public void UpdateArea(Area area)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Areas.Update(area);
                context.SaveChanges();
            }
        }

        public void DeleteArea(int areaId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                //Kiểm tra xem Area còn Bàn không
                bool hasTables = context.Tables.Any(t => t.AreaId == areaId);
                if (hasTables)
                {
                    // Nếu còn Bàn, báo lỗi
                    throw new Exception("Cannot delete area. It still contains tables.");
                }

                // Nếu không còn Bàn, tiến hành xóa
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
