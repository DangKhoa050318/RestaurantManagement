using BusinessObjects.Models;
using System.Collections.Generic;
namespace BusinessLogicLayer.Services.Interfaces
{
    public interface ICustomerService
    {
        List<Customer> GetCustomers();
        Customer GetCustomerById(int id);
        Customer GetCustomerByName(string name);
        Customer GetCustomerByPhone(string phone);
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int id);
        List<Customer> SearchCustomer(string keyword);
    }
}