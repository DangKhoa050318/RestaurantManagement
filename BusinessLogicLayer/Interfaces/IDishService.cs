using BusinessObjects.Models;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface IDishService
    {
        List<Dish> GetDishes();
        Dish GetDishById(int id);
        List<Dish> GetDishesByCategoryId(int categoryId);
        List<Dish> SearchDishesByName(string keyword);
        void AddDish(Dish dish);
        void UpdateDish(Dish dish);
        void DeleteDish(int id);
    }
}