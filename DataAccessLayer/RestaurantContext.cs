using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAccessLayer
{
    public class RestaurantMiniManagementDbContext : DbContext
    {
        public RestaurantMiniManagementDbContext() { }

        public RestaurantMiniManagementDbContext(DbContextOptions<RestaurantMiniManagementDbContext> options)
            : base(options)
        {
        }

        // Khai báo tất cả các Bảng
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
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

            // 1. Ánh xạ Class 'Category' sang bảng 'categories'
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId).HasName("PK__categori__19093A0B79815C21");

                entity.ToTable("categories");

                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            // 2. Ánh xạ Class 'Customer' sang bảng 'customers'
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId).HasName("PK__customer__A4AE64D826E1350C");

                entity.ToTable("customers");

                entity.HasIndex(e => e.Phone, "UQ__customer__5C7E359E40C946C4").IsUnique();

                entity.Property(e => e.Fullname).HasMaxLength(100);
                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            // 3. Ánh xạ Class 'Food' (C#) sang bảng 'dishes' (DB)
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasKey(e => e.DishId).HasName("PK__dishs__18834F505B455D73");

                entity.ToTable("dishs");

                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.ImgUrl)
                    .IsUnicode(false)
                    .HasColumnName("ImgURL");
                entity.Property(e => e.Name).HasMaxLength(150);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.UnitOfCalculation)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category).WithMany(p => p.Dishes)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__dishs__CategoryI__31EC6D26");
            });

            // 4. Ánh xạ Class 'Order' sang bảng 'orders'
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId).HasName("PK__orders__C3905BCF32CB37EF");

                entity.ToTable("orders");

                entity.Property(e => e.OrderTime)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                
                // ✅ PaymentTime - nullable
                entity.Property(e => e.PaymentTime)
                    .HasColumnType("datetime")
                    .IsRequired(false);
                
                entity.Property(e => e.Status)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValue("Scheduled");
                
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__orders__Customer__3A81B327");

                entity.HasOne(d => d.Table).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.TableId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__orders__TableId__398D8EEE");
            });

            // 5. Ánh xạ Class 'OrderDetail' sang bảng 'orderdetails'
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId).HasName("PK__orderdet__D3B9D36C7EFFF098");

                entity.ToTable("orderdetails");

                entity.Property(e => e.Quantity).HasDefaultValue(1);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Dish).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.DishId)
                    .HasConstraintName("FK__orderdeta__DishI__412EB0B6");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__orderdeta__Order__403A8C7D");
            });

            // 6. Ánh xạ Class 'Table' sang bảng 'tables'
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(e => e.TableId).HasName("PK__tables__7D5F01EE27439C66");

                entity.ToTable("tables");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.TableName)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Area).WithMany(p => p.Tables)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK__tables__AreaId__2C3393D0");
            });

            // 7. Ánh xạ Class 'Area' sang bảng 'areas'
            modelBuilder.Entity<Area>(entity =>
            {
                entity.HasKey(e => e.AreaId).HasName("PK__areas__70B82048FEB1C266");

                entity.ToTable("areas");

                entity.Property(e => e.AreaId)
                    .ValueGeneratedOnAdd(); // Auto-increment identity

                entity.Property(e => e.AreaName).HasMaxLength(50);
                entity.Property(e => e.AreaStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Using");
            });

        }
    }
}