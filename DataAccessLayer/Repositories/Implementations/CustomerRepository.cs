using BusinessObjects.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
            using (var context = new RestaurantDbContext())
            {
                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var context = new RestaurantDbContext())
            {
                context.Customers.Update(customer);
                context.SaveChanges();
            }
        }

        public void DeleteCustomer(int id)
        {
            using (var context = new RestaurantDbContext())
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
            using (var context = new RestaurantDbContext())
            {
                return context.Customers.Find(id);
            }
        }

        public Customer GetCustomerByName(string name)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Customers.FirstOrDefault(c => c.Fullname == name);
            }
        }

        public Customer GetCustomerByPhone(string phone)
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Customers.FirstOrDefault(c => c.Phone == phone);
            }
        }

        public List<Customer> GetCustomers()
        {
            using (var context = new RestaurantDbContext())
            {
                return context.Customers.ToList();
            }
        }

    }
}
