using Xunit;
using Moq;
using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Services.Implementations;
using System;

namespace BusinessLogicLayer.Tests
{
    public class TableServiceTests
    {
        private readonly Mock<ITableRepository> _mockRepo;
        private readonly TableService _service;

        public TableServiceTests()
        {
            _mockRepo = new Mock<ITableRepository>();
            _service = new TableService(_mockRepo.Object);
        }

        // Dùng [Theory] để test tất cả các status hợp lệ
        [Theory]
        [InlineData("Empty")]
        [InlineData("Using")]
        [InlineData("Booked")]
        [InlineData("Maintenance")]
        public void UpdateTableStatus_WithValidStatus_ShouldUpdateAndCallRepository(string validStatus)
        {
            // 1. Arrange
            var table = new Table { TableId = 1, Status = "OldStatus" };
            _mockRepo.Setup(repo => repo.GetTableById(1)).Returns(table);

            // 2. Act
            _service.UpdateTableStatus(1, validStatus);

            // 3. Assert
            // Kiểm tra đối tượng table đã bị thay đổi status
            Assert.Equal(validStatus, table.Status);
            // Kiểm tra repo được gọi để lưu
            _mockRepo.Verify(repo => repo.UpdateTable(table), Times.Once());
        }

        [Fact]
        public void UpdateTableStatus_WithInvalidStatus_ShouldThrowArgumentException()
        {
            // 1. Arrange
            string invalidStatus = "Broken"; // Trạng thái không hợp lệ

            // 2. Act & 3. Assert
            Assert.Throws<ArgumentException>(() => _service.UpdateTableStatus(1, invalidStatus));
            _mockRepo.Verify(repo => repo.GetTableById(It.IsAny<int>()), Times.Never());
            _mockRepo.Verify(repo => repo.UpdateTable(It.IsAny<Table>()), Times.Never());
        }

        [Fact]
        public void UpdateTableStatus_ForNonExistingTable_ShouldThrowException()
        {
            // 1. Arrange
            // Dạy cho repo trả về null khi tìm bàn không tồn tại
            _mockRepo.Setup(repo => repo.GetTableById(999)).Returns((Table)null);

            // 2. Act & 3. Assert
            var ex = Assert.Throws<Exception>(() => _service.UpdateTableStatus(999, "Empty"));
            Assert.Equal("Table not found.", ex.Message);
        }
    }
}