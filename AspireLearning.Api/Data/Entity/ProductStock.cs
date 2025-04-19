namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalInterface;

public class ProductStock : IBaseEntity, IDeletableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal MaximumQuantity { get; set; }

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<StockTransaction>? Transactions { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }

    public bool IsDeleted { get; set; }
}

public class ProductStockConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.ToTable("ProductStocks");
            entity.HasKey(ps => ps.Id);

            entity.HasOne(ps => ps.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ps => ps.Warehouse)
                .WithMany()
                .HasForeignKey(ps => ps.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 