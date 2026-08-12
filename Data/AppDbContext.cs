using Microsoft.EntityFrameworkCore;
using SmartPOS.Models;
using SmartPOS.Models.Enums;

namespace SmartPOS.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<FaceSample> FaceSamples => Set<FaceSample>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEmployees(modelBuilder);
        ConfigureSalesAndInventory(modelBuilder);
        SeedDevelopmentData(modelBuilder);
    }

    private static void ConfigureEmployees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.EmployeeCode).IsUnique();
        });

        modelBuilder.Entity<FaceSample>(entity =>
        {
            entity.Property(x => x.ImagePath).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Employee).WithMany(x => x.FaceSamples).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
            entity.HasOne(x => x.Employee).WithMany(x => x.Attendances).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSalesAndInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.CategoryName).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Barcode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CaseBarcode).HasMaxLength(50);
            entity.Property(x => x.RetailUnitName).HasMaxLength(30).HasDefaultValue("Cái").IsRequired();
            entity.Property(x => x.ImportUnitName).HasMaxLength(30).HasDefaultValue("Thùng").IsRequired();
            entity.Property(x => x.UnitsPerImportUnit).HasDefaultValue(1).IsRequired();
            entity.Property(x => x.ImagePath).HasMaxLength(500);
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);
            entity.HasIndex(x => x.Barcode).IsUnique();
            entity.HasIndex(x => x.CaseBarcode).IsUnique().HasFilter("[CaseBarcode] IS NOT NULL");
            entity.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Product_CostPrice", "[CostPrice] >= 0");
                table.HasCheckConstraint("CK_Product_SellingPrice", "[SellingPrice] >= 0");
                table.HasCheckConstraint("CK_Product_StockQuantity", "[StockQuantity] >= 0");
                table.HasCheckConstraint("CK_Product_MinStockAlert", "[MinStockAlert] >= 0");
                table.HasCheckConstraint("CK_Product_UnitsPerImportUnit", "[UnitsPerImportUnit] >= 1");
            });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.FinalAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.Employee).WithMany(x => x.Orders).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Order).WithMany(x => x.OrderDetails).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany(x => x.OrderDetails).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.ImportUnitNameSnapshot).HasMaxLength(30);
            entity.Property(x => x.UnitCostSnapshot).HasPrecision(18, 2);
            entity.HasOne(x => x.Product).WithMany(x => x.StockTransactions).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void SeedDevelopmentData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasData(
            new Employee { EmployeeId = 1, EmployeeCode = "NV0001", FullName = "Nguyễn Minh Anh", Email = "manager@smartpos.local", Phone = "0901000001", Role = EmployeeRole.Manager, PasswordHash = "DEVELOPMENT-ONLY-NOT-A-PASSWORD", IsActive = true },
            new Employee { EmployeeId = 2, EmployeeCode = "NV0002", FullName = "Trần Quốc Bảo", Email = "cashier@smartpos.local", Phone = "0901000002", Role = EmployeeRole.Cashier, PasswordHash = "DEVELOPMENT-ONLY-NOT-A-PASSWORD", IsActive = true });

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Đồ uống" }, new Category { CategoryId = 2, CategoryName = "Thực phẩm" },
            new Category { CategoryId = 3, CategoryName = "Đồ gia dụng" }, new Category { CategoryId = 4, CategoryName = "Chăm sóc cá nhân" },
            new Category { CategoryId = 5, CategoryName = "Khác" });

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, CategoryId = 1, ProductName = "Nước suối Aquafina 500ml", Barcode = "8934588012221", CaseBarcode = "18934588012228", RetailUnitName = "Chai", ImportUnitName = "Thùng", UnitsPerImportUnit = 24, CostPrice = 4000m, SellingPrice = 7000m, StockQuantity = 48, MinStockAlert = 10 },
            new Product { ProductId = 2, CategoryId = 1, ProductName = "Coca-Cola lon 330ml", Barcode = "8934588012222", CaseBarcode = "18934588012229", RetailUnitName = "Lon", ImportUnitName = "Thùng", UnitsPerImportUnit = 24, CostPrice = 8000m, SellingPrice = 12000m, StockQuantity = 36, MinStockAlert = 10 },
            new Product { ProductId = 3, CategoryId = 1, ProductName = "Trà xanh C2 360ml", Barcode = "8934588012223", CaseBarcode = "18934588012230", RetailUnitName = "Chai", ImportUnitName = "Thùng", UnitsPerImportUnit = 24, CostPrice = 7000m, SellingPrice = 11000m, StockQuantity = 8, MinStockAlert = 10 },
            new Product { ProductId = 4, CategoryId = 2, ProductName = "Mì Hảo Hảo tôm chua cay", Barcode = "8934588012224", CaseBarcode = "18934588012231", RetailUnitName = "Gói", ImportUnitName = "Thùng", UnitsPerImportUnit = 30, CostPrice = 3500m, SellingPrice = 5000m, StockQuantity = 80, MinStockAlert = 20 },
            new Product { ProductId = 5, CategoryId = 2, ProductName = "Bánh Oreo Original", Barcode = "8934588012225", CostPrice = 9000m, SellingPrice = 14000m, StockQuantity = 25, MinStockAlert = 8 },
            new Product { ProductId = 6, CategoryId = 2, ProductName = "Sữa tươi Vinamilk 180ml", Barcode = "8934588012226", CostPrice = 6500m, SellingPrice = 9000m, StockQuantity = 12, MinStockAlert = 12 },
            new Product { ProductId = 7, CategoryId = 3, ProductName = "Khăn giấy rút Paseo", Barcode = "8934588012227", CostPrice = 18000m, SellingPrice = 25000m, StockQuantity = 15, MinStockAlert = 6 },
            new Product { ProductId = 8, CategoryId = 3, ProductName = "Nước rửa chén Sunlight 750g", Barcode = "8934588012228", CostPrice = 24000m, SellingPrice = 32000m, StockQuantity = 5, MinStockAlert = 8 },
            new Product { ProductId = 9, CategoryId = 4, ProductName = "Kem đánh răng P/S 180g", Barcode = "8934588012229", CostPrice = 17000m, SellingPrice = 24000m, StockQuantity = 21, MinStockAlert = 7 },
            new Product { ProductId = 10, CategoryId = 4, ProductName = "Dầu gội Clear Men 170ml", Barcode = "8934588012230", CostPrice = 42000m, SellingPrice = 55000m, StockQuantity = 0, MinStockAlert = 5 },
            new Product { ProductId = 11, CategoryId = 5, ProductName = "Pin AA Energizer vỉ 2", Barcode = "8934588012231", CostPrice = 28000m, SellingPrice = 38000m, StockQuantity = 9, MinStockAlert = 5 },
            new Product { ProductId = 12, CategoryId = 5, ProductName = "Túi đựng rác tự hủy", Barcode = "8934588012232", CostPrice = 12000m, SellingPrice = 18000m, StockQuantity = 30, MinStockAlert = 10 });
    }
}
