using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories.Implementations
{
    public class FoodRepository : IFoodRepository
    {
        private static FoodRepository instance = null;
        private static readonly object instanceLock = new object();
        private FoodRepository() { }
        public static FoodRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new FoodRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddFood(Food food)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Foods.Add(food);
                context.SaveChanges();
            }
        }

        public void UpdateFood(Food food)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Foods.Update(food);
                context.SaveChanges();
            }
        }

        public void DeleteFood(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                // Logic: Không xóa món ăn đã có trong OrderDetail
                bool isInOrder = context.OrderDetails.Any(od => od.FoodId == id);
                if (isInOrder)
                {
                    throw new Exception("Cannot delete food item. It is part of an existing order.");
                }

                var food = context.Foods.Find(id);
                if (food != null)
                {
                    context.Foods.Remove(food);
                    context.SaveChanges();
                }
            }
        }

        public Food GetFoodById(int id)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Foods.Include(f => f.Category)
                                    .FirstOrDefault(f => f.FoodId == id);
            }
        }

        public List<Food> GetFoodsByCategoryId(int categoryId)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Foods.Include(f => f.Category)
                                    .Where(f => f.CategoryId == categoryId)
                                    .ToList();
            }
        }

        public List<Food> GetFoods()
        {
            using (var context = new RestaurantDbContext())
            {
                // Dùng Include để lấy tên Category
                return context.Foods.Include(f => f.Category).ToList();
            }
        }

       
    }
}
