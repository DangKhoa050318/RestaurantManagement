using Xunit;
using Moq;
using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Services.Implementations;
using System;

namespace BusinessLogicLayer.Tests
{
    public class DishServiceTests
    {
        private readonly Mock<IDishRepository> _mockRepo;
        private readonly DishService _service;

        public DishServiceTests()
        {
            _mockRepo = new Mock<IDishRepository>();
            _service = new DishService(_mockRepo.Object);
        }

        [Fact]
        public void AddDish_WithEmptyName_ShouldThrowArgumentException()
        {
            var dish = new Dish { Name = "", Price = 10000 };

            Assert.Throws<ArgumentException>(() => _service.AddDish(dish));
            _mockRepo.Verify(repo => repo.AddDish(It.IsAny<Dish>()), Times.Never());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10000)]
        public void AddDish_WithZeroOrNegativePrice_ShouldThrowArgumentException(decimal invalidPrice)
        {
            var dish = new Dish { Name = "Cơm Gà", Price = invalidPrice };

            Assert.Throws<ArgumentException>(() => _service.AddDish(dish));
            _mockRepo.Verify(repo => repo.AddDish(It.IsAny<Dish>()), Times.Never());
        }

        [Fact]
        public void AddDish_WithEmptyUnit_ShouldSetDefaultAndCallRepository()
        {
            var dish = new Dish
            {
                Name = "Phở Bò",
                Price = 50000,
                UnitOfCalculation = "" 
            };

            _service.AddDish(dish);

            Assert.Equal("portion", dish.UnitOfCalculation);
            _mockRepo.Verify(repo => repo.AddDish(dish), Times.Once());
        }

        [Fact]
        public void AddDish_WithValidData_ShouldCallRepository()
        {
            var dish = new Dish
            {
                Name = "Bún Bò",
                Price = 45000,
                UnitOfCalculation = "Tô"
            };

            _service.AddDish(dish);

            Assert.Equal("Tô", dish.UnitOfCalculation);
            _mockRepo.Verify(repo => repo.AddDish(dish), Times.Once());
        }
    }
}