using System.Collections.Generic;
using BusinessObjects.Models;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IAreaRepository
    {
        List<Area> GetAreas();
        Area GetAreaById(int areaId);
        void AddArea(Area area); 
        void UpdateArea(Area area); 
        void DeleteArea(int areaId);
    }
}
