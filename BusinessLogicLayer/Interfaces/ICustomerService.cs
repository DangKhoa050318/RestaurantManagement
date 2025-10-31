using BusinessObjects.Models;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface ICustomerService
    {
        List<Customer> GetCustomers();
        Customer GetCustomerById(int id);
        Customer GetCustomerByPhone(string phone);
        List<Customer> GetCustomerByName(string name); 
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int id);
        List<Customer> SearchCustomer(string keyword);
    }
}