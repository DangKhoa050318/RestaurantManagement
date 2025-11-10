using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Services.Interfaces;
using System; 

namespace Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public List<Customer> GetCustomers() => _customerRepository.GetCustomers();

        public Customer GetCustomerById(int id) => _customerRepository.GetCustomerById(id);

        public Customer GetCustomerByPhone(string phone) => _customerRepository.GetCustomerByPhone(phone);

        public List<Customer> GetCustomerByName(string name) => _customerRepository.GetCustomerByName(name);

        public void AddCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Fullname))
                throw new ArgumentException("Customer name cannot be empty.");

            // Kiểm tra số điện thoại trùng lặp
            if (!string.IsNullOrWhiteSpace(customer.Phone))
            {
                var existing = _customerRepository.GetCustomerByPhone(customer.Phone);
                if (existing != null)
                    throw new InvalidOperationException($"Phone number '{customer.Phone}' is already registered.");
            }

            _customerRepository.AddCustomer(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            var existing = _customerRepository.GetCustomerById(customer.CustomerId);
            if (existing == null)
            {
                throw new Exception("Customer not found.");
            }

            // Cập nhật các trường
            existing.Fullname = customer.Fullname;
            existing.Phone = customer.Phone;

            _customerRepository.UpdateCustomer(existing);
        }

        public void DeleteCustomer(int id)
        {
            try
            {
                _customerRepository.DeleteCustomer(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Customer> SearchCustomer(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _customerRepository.GetCustomers();

            // Phân biệt tìm theo tên hay theo SĐT
            if (long.TryParse(keyword, out _)) // Nếu keyword là SỐ (dùng long cho SĐT)
            {
                var customer = _customerRepository.GetCustomerByPhone(keyword);
                // Trả về 1 danh sách (List)
                return customer != null ? new List<Customer> { customer } : new List<Customer>();
            }
            else // Nếu keyword là CHỮ
            {
                return _customerRepository.GetCustomerByName(keyword);
            }
        }
    }
}