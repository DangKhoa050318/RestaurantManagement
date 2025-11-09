using Xunit;
using Moq;
using BusinessLogicLayer.Services.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using BusinessObjects.Models;
using System.Collections.Generic;
using System;

namespace BusinessLogicLayer.Tests
{
    public class AreaServiceTests
    {
        private readonly Mock<IAreaRepository> _mockRepo;
        private readonly AreaService _service;

        public AreaServiceTests()
        {
            _mockRepo = new Mock<IAreaRepository>();
            _service = new AreaService(_mockRepo.Object);
        }

        [Fact]
        public void AddArea_WithValidName_ShouldCallRepositoryAdd()
        {
            var newArea = new Area { AreaName = "Khu Vườn" };

            _service.AddArea(newArea);

            _mockRepo.Verify(repo => repo.AddArea(It.IsAny<Area>()), Times.Once());
        }

        [Fact]
        public void AddArea_WithEmptyName_ShouldThrowArgumentException()
        {
            var invalidArea = new Area { AreaName = "" }; // Tên rỗng

            Assert.Throws<ArgumentException>(() => _service.AddArea(invalidArea));

            _mockRepo.Verify(repo => repo.AddArea(It.IsAny<Area>()), Times.Never());
        }

        [Fact]
        public void GetAreasByStatus_WithValidStatus_ShouldReturnFilteredList()
        {
            var allAreas = new List<Area>
            {
                new Area { AreaId = 1, AreaName = "Trong Nhà", AreaStatus = "Active" },
                new Area { AreaId = 2, AreaName = "Ngoài Trời", AreaStatus = "Inactive" },
                new Area { AreaId = 3, AreaName = "Sân Thượng", AreaStatus = "Active" }
            };

            _mockRepo.Setup(repo => repo.GetAreas()).Returns(allAreas);

            var results = _service.GetAreasByStatus("Active");

            Assert.NotNull(results);
            Assert.Equal(2, results.Count); // Phải trả về 2
            Assert.Contains(results, a => a.AreaId == 1);
            Assert.Contains(results, a => a.AreaId == 3);
        }
    }
}