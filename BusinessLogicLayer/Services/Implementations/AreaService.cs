using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;

namespace BusinessLogicLayer.Services.Implementations
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;
        public AreaService()
        {
            _areaRepository = AreaRepository.Instance;
        }
        public List<Area> GetAreas() => _areaRepository.GetAreas();
    }
}
