using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using Services.Interfaces; 
using System; 
using System.Linq; 

namespace Services.Implementations 
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService()
        {
            _categoryRepository = CategoryRepository.Instance;
        }

        public List<Category> GetCategories() => _categoryRepository.GetCategories();


        public Category GetCategoryById(int id)
        {
            return _categoryRepository.GetCategoryById(id);
        }

        public void AddCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be empty.");

            _categoryRepository.AddCategory(category);
        }

        public void UpdateCategory(Category category)
        {
            var existing = _categoryRepository.GetCategoryById(category.CategoryId);
            if (existing == null)
            {
                throw new Exception("Category not found.");
            }

            existing.Name = category.Name;
            existing.Description = category.Description;

            // Đẩy đối tượng đã cập nhật xuống Repository
            _categoryRepository.UpdateCategory(existing);
        }

        public void DeleteCategory(int id)
        {
            try
            {
                _categoryRepository.DeleteCategory(id);
            }
            catch (Exception ex)
            {
                // Chuyển tiếp lỗi (ví dụ: "Cannot delete category...")
                throw new Exception(ex.Message);
            }
        }

        public List<Category> SearchCategoriesByName(string keyword)
        {
            var allCategories = _categoryRepository.GetCategories();

            if (string.IsNullOrWhiteSpace(keyword))
                return allCategories;

            return allCategories
                .Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}