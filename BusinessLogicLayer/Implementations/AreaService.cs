using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces; 
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System; 
using System.Linq;

namespace BusinessLogicLayer.Services.Implementations
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;

        public AreaService(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public List<Area> GetAreas() => _areaRepository.GetAreas();

        public Area GetAreaById(int areaId) => _areaRepository.GetAreaById(areaId);

        public void AddArea(Area area)
        {
            if (string.IsNullOrWhiteSpace(area.AreaName))
                throw new ArgumentException("Area name cannot be empty.");

            _areaRepository.AddArea(area);
        }

        public void UpdateArea(Area area)
        {
            // Lấy đối tượng gốc
            var existing = _areaRepository.GetAreaById(area.AreaId);
            if (existing == null)
            {
                throw new Exception("Area not found.");
            }

            // Cập nhật các trường (logic nghiệp vụ)
            existing.AreaName = area.AreaName;
            existing.AreaStatus = area.AreaStatus;

            // Đẩy đối tượng đã cập nhật xuống Repository
            _areaRepository.UpdateArea(existing);
        }

        public void DeleteArea(int areaId)
        {
            try
            {
                _areaRepository.DeleteArea(areaId);
            }
            catch (Exception ex)
            {
                // Chuyển tiếp lỗi (ví dụ: "Cannot delete area...")
                throw new Exception(ex.Message);
            }
        }

        public List<Area> GetAreasByStatus(string status)
        {
            var all = _areaRepository.GetAreas();
            return all.Where(a => a.AreaStatus.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}