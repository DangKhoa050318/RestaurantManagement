using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System; 

namespace Services.Implementations 
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _dishRepository;

        public DishService(IDishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        public List<Dish> GetDishes() => _dishRepository.GetDish();

        public Dish GetDishById(int id) => _dishRepository.GetDishById(id);

        public List<Dish> GetDishesByCategoryId(int categoryId) => _dishRepository.GetDishByCategoryId(categoryId);

        public void AddDish(Dish dish)
        {
            System.Diagnostics.Debug.WriteLine($"🔹 DishService.AddDish called with: Name='{dish.Name}', Price={dish.Price}, CategoryId={dish.CategoryId}, Unit='{dish.UnitOfCalculation}'");
            
            if (string.IsNullOrWhiteSpace(dish.Name))
            {
                System.Diagnostics.Debug.WriteLine("❌ Validation failed: Dish name is empty");
                throw new ArgumentException("Dish name cannot be empty.");
            }

            if (dish.Price <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Validation failed: Dish price {dish.Price} <= 0");
                throw new ArgumentException("Dish price must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(dish.UnitOfCalculation))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Unit is empty, setting default to 'portion'");
                dish.UnitOfCalculation = "portion"; // Đặt giá trị mặc định
            }

            System.Diagnostics.Debug.WriteLine("✅ Validation passed, calling repository...");
            _dishRepository.AddDish(dish);
            System.Diagnostics.Debug.WriteLine("✅ Repository.AddDish completed");
        }

        public void UpdateDish(Dish dish)
        {
            var existing = _dishRepository.GetDishById(dish.DishId);
            if (existing == null)
            {
                throw new Exception("Dish not found.");
            }

            existing.Name = dish.Name;
            existing.Price = dish.Price;
            existing.UnitOfCalculation = dish.UnitOfCalculation;
            existing.Description = dish.Description;
            existing.ImgUrl = dish.ImgUrl; 
            existing.CategoryId = dish.CategoryId;

            _dishRepository.UpdateDish(existing);
        }

        public void DeleteDish(int id)
        {
            try
            {
                _dishRepository.DeleteDish(id);
            }
            catch (Exception ex)
            {
                // Chuyển tiếp lỗi (ví dụ: "Cannot delete dish...")
                throw new Exception(ex.Message);
            }
        }

        public List<Dish> SearchDishesByName(string keyword)
        {
            var all = _dishRepository.GetDish();

            if (string.IsNullOrWhiteSpace(keyword))
                return all;

            return all.Where(d => d.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}