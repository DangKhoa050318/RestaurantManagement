using Xunit;
using Moq;
using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using Services.Implementations; 
using System.Collections.Generic;
using System;

namespace BusinessLogicLayer.Tests
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockRepo;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _mockRepo = new Mock<ICustomerRepository>();
            _service = new CustomerService(_mockRepo.Object);
        }

        [Fact]
        public void AddCustomer_WithValidData_ShouldCallRepository()
        {
            var newCustomer = new Customer { Fullname = "Nguyễn Văn A", Phone = "0905123456" };

            _mockRepo.Setup(repo => repo.GetCustomerByPhone("0905123456")).Returns((Customer)null);

            _service.AddCustomer(newCustomer);

            _mockRepo.Verify(repo => repo.AddCustomer(newCustomer), Times.Once());
        }

        [Fact]
        public void AddCustomer_WithDuplicatePhone_ShouldThrowInvalidOperationException()
        {
            var existingCustomer = new Customer { CustomerId = 1, Phone = "0905123456" };
            var newCustomer = new Customer { Fullname = "Nguyễn Văn B", Phone = "0905123456" };

            _mockRepo.Setup(repo => repo.GetCustomerByPhone("0905123456")).Returns(existingCustomer);

            Assert.Throws<InvalidOperationException>(() => _service.AddCustomer(newCustomer));

            _mockRepo.Verify(repo => repo.AddCustomer(It.IsAny<Customer>()), Times.Never());
        }

        [Fact]
        public void AddCustomer_WithEmptyName_ShouldThrowArgumentException()
        {
            var customer = new Customer { Fullname = "", Phone = "0905123456" };

            Assert.Throws<ArgumentException>(() => _service.AddCustomer(customer));
        }

        [Fact]
        public void SearchCustomer_WhenKeywordIsNumber_ShouldCallGetCustomerByPhone()
        {
            string keyword = "0905123456";
            var customer = new Customer { Fullname = "A", Phone = keyword };
            _mockRepo.Setup(repo => repo.GetCustomerByPhone(keyword)).Returns(customer);

            var result = _service.SearchCustomer(keyword);

            Assert.Single(result); 
            Assert.Equal(keyword, result.First().Phone);
            _mockRepo.Verify(repo => repo.GetCustomerByPhone(keyword), Times.Once());
            _mockRepo.Verify(repo => repo.GetCustomerByName(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void SearchCustomer_WhenKeywordIsText_ShouldCallGetCustomerByName()
        {
            string keyword = "An";
            var customers = new List<Customer> { new Customer { Fullname = "An" } };
            _mockRepo.Setup(repo => repo.GetCustomerByName(keyword)).Returns(customers);

            var result = _service.SearchCustomer(keyword);

            Assert.Single(result);
            Assert.Equal("An", result.First().Fullname);
            _mockRepo.Verify(repo => repo.GetCustomerByName(keyword), Times.Once());
            _mockRepo.Verify(repo => repo.GetCustomerByPhone(It.IsAny<string>()), Times.Never());
        }
    }
}