using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Warehouse.Data.Entity;

public class Warehouse : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;

    public ICollection<ProductStock> Stocks { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}

public class WarehouseConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            
            entity.HasKey(w => w.Id);
            
            entity.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(w => w.Address)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasIndex(w => w.TenantId).IsUnique(false);
            
            entity.HasMany(w => w.Stocks)
                .WithOne(s => s.Warehouse)
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}