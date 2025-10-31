using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogicLayer.Services.Implementations
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _foodRepository;
        public FoodService()
        {
            _foodRepository = FoodRepository.Instance;
        }

        public void AddFood(Food food) => _foodRepository.AddFood(food);
        public void UpdateFood(Food food) => _foodRepository.UpdateFood(food);
        public void DeleteFood(int id) => _foodRepository.DeleteFood(id);
        public Food GetFoodById(int id) => _foodRepository.GetFoodById(id);
        public List<Food> GetFoodByCategoryId(int categoryId) => _foodRepository.GetFoodsByCategoryId(categoryId);
        public List<Food> GetFoods() => _foodRepository.GetFoods();

        public List<Food> SearchFood(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _foodRepository.GetFoods();

            string lowerKeyword = keyword.ToLower();
            return _foodRepository.GetFoods()
                .Where(f => f.Name.ToLower().Contains(lowerKeyword))
                .ToList();
        }
    }
}