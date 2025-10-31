using BusinessObjects.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IDishRepository
    {
        List<Dish> GetDish();
        Dish GetDishById(int id);
        List<Dish> GetDishByCategoryId(int categoryId);
        void AddDish(Dish dish);
        void UpdateDish(Dish dish);
        void DeleteDish(int id);
    }
}
