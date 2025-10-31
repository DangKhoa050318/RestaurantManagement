using BusinessObjects.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IFoodRepository
    {
        List<Food> GetFoods();
        Food GetFoodById(int id);
        List<Food> GetFoodsByCategoryId(int categoryId); //New
        void AddFood(Food food);
        void UpdateFood(Food food);
        void DeleteFood(int id);
    }
}
