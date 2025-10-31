using System.Collections.Generic;
using BusinessObjects.Models;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        List<Area> GetAreas();
    }
}
