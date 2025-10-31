using BusinessObjects.Models;
using System.Collections.Generic;

namespace BusinessLogicLayer.Services.Interfaces
{
    public interface IAreaService
    {
        List<Area> GetAreas();
        Area GetAreaById(int areaId); 
        void AddArea(Area area);
        void UpdateArea(Area area);
        void DeleteArea(int areaId);
        List<Area> GetAreasByStatus(string status);
    }
}