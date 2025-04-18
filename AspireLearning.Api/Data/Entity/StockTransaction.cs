/*namespace AspireLearning.Api.Data.Entity;

using ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

public class StockTransaction
{
    public Guid Id { get; set; }
    public Guid ProductStockId { get; set; }
    
    public int QuantityChange { get; set; }
    public DateTime TransactionDate { get; set; }
    //public TransactionType Type { get; set; }
    
    public string? Description { get; set; }
    
    public ProductStock? ProductStock { get; set; }
}

public class StockTransactionConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable("StockTransactions");

            entity.HasKey(st => st.Id);

            entity.Property(st => st.QuantityChange).IsRequired();

            entity.Property(st => st.TransactionDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            //entity.Property(st => st.Type)
            //    .HasConversion<string>()
            //    .HasMaxLength(50)
            //    .IsRequired();

            entity.Property(st => st.Description)
                .HasMaxLength(500)
                .IsRequired(false);
        });
    }
}*/