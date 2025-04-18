/*namespace AspireLearning.Api.Data.Entity;

using ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

public class ProductStock : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    //public StockStatus Status { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; } 
    public int Quantity { get; set; }
    
    public ICollection<StockTransaction>? Transactions { get; set; }
    
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}

public class ProductStockConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.ToTable("ProductStocks");

            entity.HasKey(ps => ps.Id);

            entity.HasIndex(ps => new { ps.ProductId, ps.WarehouseId, ps.BatchNumber, ps.SerialNumber})
                  .IsUnique();

            entity.Property(ps => ps.BatchNumber)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(ps => ps.SerialNumber)
                  .HasMaxLength(100)
                  .IsRequired(false);

            //entity.Property(ps => ps.Status)
            //      .HasConversion<string>()
            //      .HasMaxLength(50);

            entity.Property(ps => ps.Quantity)
                  .IsRequired();

            entity.HasOne(ps => ps.Product)
                  .WithMany(p => p.Stocks)
                  .HasForeignKey(ps => ps.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            //entity.HasOne(ps => ps.Warehouse)
            //      .WithMany(w => w.Stocks)
            //      .HasForeignKey(ps => ps.WarehouseId)
            //      .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(ps => ps.Transactions)
                  .WithOne(t => t.ProductStock)
                  .HasForeignKey(t => t.ProductStockId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}*/