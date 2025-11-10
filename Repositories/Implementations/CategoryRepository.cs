using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

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
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Categories.Include(c => c.Dishes).ToList();
            }
        }

        public Category GetCategoryById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Categories
                              .Include(c => c.Dishes)
                              .FirstOrDefault(c => c.CategoryId == id);
            }
        }

        public void AddCategory(Category category)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Categories.Add(category);
                context.SaveChanges();
            }
        }

        public void UpdateCategory(Category category)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Categories.Update(category);
                context.SaveChanges();
            }
        }

        public void DeleteCategory(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                // Kiểm tra xem Category còn Dish không
                bool hasFoods = context.Dishes.Any(f => f.CategoryId == id);
                if (hasFoods)
                {
                    throw new Exception("Cannot delete category. It still contains dish items.");
                }

                var category = context.Categories.Find(id);
                if (category != null)
                {
                    context.Categories.Remove(category);
                    context.SaveChanges();
                }
            }
        }
    }
}