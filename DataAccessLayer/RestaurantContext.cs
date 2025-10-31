using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAccessLayer
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext() { }

        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        // Khai báo tất cả các Bảng
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Area> Areas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Lấy chuỗi kết nối từ appsettings.json
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                IConfiguration configuration = builder.Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- BẮT ĐẦU ÁNH XẠ (MAPPING) ---

            // 1. Ánh xạ Class 'Category' sang bảng 'categories'
            modelBuilder.Entity<Category>().ToTable("categories");

            // 2. Ánh xạ Class 'Customer' sang bảng 'customers'
            modelBuilder.Entity<Customer>().ToTable("customers");

            // 3. Ánh xạ Class 'Food' (C#) sang bảng 'dishes' (DB)
            modelBuilder.Entity<Food>(entity =>
            {
                entity.ToTable("dishes"); // Tên bảng
                entity.Property(e => e.FoodId).HasColumnName("DishId"); // Khóa chính
            });

            // 4. Ánh xạ Class 'Order' sang bảng 'orders'
            modelBuilder.Entity<Order>().ToTable("orders");

            // 5. Ánh xạ Class 'OrderDetail' sang bảng 'orderdetails'
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("orderdetails");
                // Ánh xạ khóa ngoại 'FoodId' (C#) sang cột 'DishId' (DB)
                entity.Property(e => e.FoodId).HasColumnName("DishId");
            });

            // 6. Ánh xạ Class 'Table' sang bảng 'tables'
            modelBuilder.Entity<Table>(entity =>
            {
                entity.ToTable("tables");
                // Ánh xạ 'TableName' (C#) sang 'TableName' (DB)
                entity.Property(e => e.TableName).HasColumnName("TableName");
            });

            // 7. Ánh xạ Class 'Area' sang bảng 'areas'
            modelBuilder.Entity<Area>().ToTable("areas");

        }
    }
}