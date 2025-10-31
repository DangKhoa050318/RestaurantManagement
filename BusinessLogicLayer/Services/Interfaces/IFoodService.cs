using BusinessObjects.Models;
using System.Collections.Generic;
namespace BusinessLogicLayer.Services.Interfaces
{
    public interface IFoodService
    {
        List<Food> GetFoods();
        Food GetFoodById(int id);
        List<Food> GetFoodByCategoryId(int categoryId);
        void AddFood(Food food);
        void UpdateFood(Food food);
        void DeleteFood(int id);
        List<Food> SearchFood(string keyword); // Thêm hàm Search
    }
}