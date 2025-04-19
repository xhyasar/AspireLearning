namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalInterface;

public class Product : IBaseEntity, IDeletableEntity, ITenantIsolatedEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? SKU { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Unit { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<ProductStock>? Stocks { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }

    public bool IsDeleted { get; set; }
}

public class ProductConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(p => p.Description)
                .HasMaxLength(1000);
                
            entity.Property(p => p.Barcode)
                .HasMaxLength(50);
                
            entity.Property(p => p.SKU)
                .HasMaxLength(50);
                
            entity.Property(p => p.Unit)
                .HasMaxLength(20);

            entity.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 