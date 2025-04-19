using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Data.Entity;

public class WarehouseCategory : IBaseEntity, IDeletableEntity, ITenantIsolatedEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;

    public Tenant? Tenant { get; set; }
    public ICollection<Warehouse>? Warehouses { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }

    public bool IsDeleted { get; set; }
}

public class WarehouseCategoryConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WarehouseCategory>(entity =>
        {
            entity.ToTable("WarehouseCategories");
            entity.HasKey(wc => wc.Id);
            entity.Property(wc => wc.Name).IsRequired().HasMaxLength(255);

            entity.HasOne(wc => wc.Tenant)
                .WithMany()
                .HasForeignKey(wc => wc.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

