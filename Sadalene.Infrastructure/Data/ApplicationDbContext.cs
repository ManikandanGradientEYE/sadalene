using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Documents;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Masters;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<OtpLog> OtpLogs => Set<OtpLog>();

    // Masters
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<PackingType> PackingTypes => Set<PackingType>();
    public DbSet<UomMaster> UomMasters => Set<UomMaster>();

    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    // Inventory
    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
    public DbSet<InventorySyncLog> InventorySyncLogs => Set<InventorySyncLog>();
    public DbSet<InventoryAdjustmentLog> InventoryAdjustmentLogs => Set<InventoryAdjustmentLog>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Documents
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Challan> Challans => Set<Challan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
