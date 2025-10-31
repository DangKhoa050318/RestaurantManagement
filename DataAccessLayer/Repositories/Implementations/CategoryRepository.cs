using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private static CategoryRepository instance = null;
        private static readonly object instanceLock = new object();
        private CategoryRepository() { }
        public static CategoryRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CategoryRepository();
                    }
                    return instance;
                }
            }
        }

        public List<Category> GetCategories()
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Categories.ToList();
            }
        }
    }
}