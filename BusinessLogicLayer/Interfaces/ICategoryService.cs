using BusinessObjects.Models;
using System.Collections.Generic;

namespace Services.Interfaces 
{
    public interface ICategoryService
    {
        List<Category> GetCategories();
        Category GetCategoryById(int id);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);
        List<Category> SearchCategoriesByName(string keyword);
    }
}