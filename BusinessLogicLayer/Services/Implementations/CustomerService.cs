using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogicLayer.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        public CustomerService()
        {
            _customerRepository = CustomerRepository.Instance;
        }

        public void AddCustomer(Customer customer) => _customerRepository.AddCustomer(customer);
        public void UpdateCustomer(Customer customer) => _customerRepository.UpdateCustomer(customer);
        public void DeleteCustomer(int id) => _customerRepository.DeleteCustomer(id);
        public Customer GetCustomerById(int id) => _customerRepository.GetCustomerById(id);
        public Customer GetCustomerByName(string name) => _customerRepository.GetCustomerByName(name);
        public Customer GetCustomerByPhone(string phone) => _customerRepository.GetCustomerByPhone(phone);
        public List<Customer> GetCustomers() => _customerRepository.GetCustomers();

        public List<Customer> SearchCustomer(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _customerRepository.GetCustomers();

            string lowerKeyword = keyword.ToLower();
            return _customerRepository.GetCustomers()
                .Where(c => c.Fullname.ToLower().Contains(lowerKeyword) ||
                            c.Phone.Contains(keyword))
                .ToList();
        }
    }
}