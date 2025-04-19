namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalInterface;

public enum TransactionType
{
    In,    // Stok girişi
    Out,   // Stok çıkışı
    Adjust // Stok düzeltme
}

public class StockTransaction : IBaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductStockId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }

    public ProductStock? ProductStock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
}

public class StockTransactionConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable("StockTransactions");
            entity.HasKey(st => st.Id);

            entity.Property(st => st.Description)
                .HasMaxLength(500);

            entity.Property(st => st.ReferenceNumber)
                .HasMaxLength(50);

            entity.HasOne(st => st.ProductStock)
                .WithMany(ps => ps.Transactions)
                .HasForeignKey(st => st.ProductStockId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 