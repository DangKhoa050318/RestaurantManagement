using BusinessObjects.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        List<Category> GetCategories();
    }
}
