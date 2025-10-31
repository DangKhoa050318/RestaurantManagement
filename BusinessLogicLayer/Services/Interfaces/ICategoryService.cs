using BusinessObjects.Models;
using System.Collections.Generic;
namespace BusinessLogicLayer.Services.Interfaces
{
    public interface ICategoryService
    {
        List<Category> GetCategories();
    }
}