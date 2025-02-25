using System.Reflection;
using AspireLearning.Warehouse.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Warehouse.Data.Context;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductStock> ProductStocks { get; set; } = null!;
    public DbSet<StockTransaction> StockTransactions { get; set; } = null!;
    public DbSet<Entity.Warehouse> Warehouses { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}