using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
            using (var context = new RestaurantDbContext())
            {
                return context.Areas.ToList();
            }
        }
    }
}
