using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataAccessLayer.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private static CustomerRepository instance = null;
        private static readonly object instanceLock = new object();
        private CustomerRepository() { }
        public static CustomerRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CustomerRepository();
                    }
                    return instance;
                }
            }
        }

        public void AddCustomer(Customer customer)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                context.Customers.Update(customer);
                context.SaveChanges();
            }
        }

        public void DeleteCustomer(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                var customer = context.Customers.Find(id);
                if (customer != null)
                {
                    context.Customers.Remove(customer);
                    context.SaveChanges();
                }
            }
        }

        public Customer GetCustomerById(int id)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Customers
                              .Include(c => c.Orders)
                              .FirstOrDefault(c => c.CustomerId == id);
            }
        }

        public List<Customer> GetCustomerByName(string name)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Customers
                              .Include(c => c.Orders)
                              .Where(c => c.Fullname.Contains(name))
                              .ToList();
            }
        }

        public Customer GetCustomerByPhone(string phone)
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Customers
                              .Include(c => c.Orders)
                              .FirstOrDefault(c => c.Phone == phone);
            }
        }

        public List<Customer> GetCustomers()
        {
            using (var context = new RestaurantMiniManagementDbContext())
            {
                return context.Customers.Include(c => c.Orders).ToList();
            }
        }

    }
}
