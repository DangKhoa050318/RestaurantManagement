using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Repositories.Implementations
{
    public class DishRepository : IDishRepository
    {
        private static DishRepository instance = null;
        private static readonly object instanceLock = new object();
        private DishRepository() { }
        public static DishRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new DishRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddDish(Dish dish)
        {
            System.Diagnostics.Debug.WriteLine($"🔸 DishRepository.AddDish called");
            System.Diagnostics.Debug.WriteLine($"   Dish data: Name='{dish.Name}', CategoryId={dish.CategoryId}, Price={dish.Price}, Unit='{dish.UnitOfCalculation}'");
            
            try
            {
                using (var context = new RestaurantMiniManagementDbContext())
                {
                    System.Diagnostics.Debug.WriteLine("   DbContext created");
                    
                    context.Dishes.Add(dish);
                    System.Diagnostics.Debug.WriteLine("   Dish added to context");
                    
                    var saveResult = context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"   ✅ SaveChanges completed. Rows affected: {saveResult}");
                    System.Diagnostics.Debug.WriteLine($"   New DishId: {dish.DishId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ❌ ERROR in AddDish: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public void UpdateDish(Dish dish)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Dishes.Update(dish);
                context.SaveChanges();
            }
        }

        public void DeleteDish(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                // Không xóa món ăn đã có trong OrderDetail
                bool isInOrder = context.OrderDetails.Any(od => od.DishId == id);
                if (isInOrder)
                {
                    throw new Exception("Cannot delete dish. It is part of an existing order.");
                }

                var dish = context.Dishes.Find(id);
                if (dish != null)
                {
                    context.Dishes.Remove(dish);
                    context.SaveChanges();
                }
            }
        }

        public Dish GetDishById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Dishes.Include(d => d.Category).Include(d=>d.OrderDetails)
                                    .FirstOrDefault(d => d.DishId == id);
            }
        }

        public List<Dish> GetDishByCategoryId(int categoryId)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Dishes.Include(d => d.Category)
                                    .Where(d => d.CategoryId == categoryId)
                                    .ToList();
            }
        }

        public List<Dish> GetDish()
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Dishes.Include(d => d.Category).Include(d=>d.OrderDetails).ToList();
            }
        }

       
    }
}
