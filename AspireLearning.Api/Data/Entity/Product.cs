/*namespace AspireLearning.Api.Data.Entity;

using ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

public class Product : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? ImageUrl { get; set; }
    
    public ICollection<ProductStock>? Stocks { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
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
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(p => p.ImageUrl)
                .HasMaxLength(255);
            
            entity.HasIndex(p => p.TenantId).IsUnique(false);
            
            entity.HasMany(p => p.Stocks)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}*/