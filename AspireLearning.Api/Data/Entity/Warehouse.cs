namespace AspireLearning.Api.Data.Entity;

using ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

public enum WarehouseStatus
{
    Active,
    Inactive,
    Warning
}

public class Warehouse : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? MapUrl { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public WarehouseStatus Status { get; set; } = WarehouseStatus.Active;

    public Guid CategoryId { get; set; }
    public WarehouseCategory? Category { get; set; }
    
    public Guid CountryId { get; set; }
    public Country? Country { get; set; }
    
    public Guid CityId { get; set; }
    public City? City { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    
    public Guid PersonInChargeId { get; set; }
    public User? PersonInCharge { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public User? CreatedByUser { get; set; }
    public User? ModifiedByUser { get; set; }
    public User? RemovedByUser { get; set; }
    
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
            
            entity.Property(w => w.Status)
                .IsRequired()
                .HasDefaultValue(WarehouseStatus.Active);

            entity.HasOne(w => w.Tenant)
                .WithMany()
                .HasForeignKey(w => w.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.PersonInCharge)
                .WithMany()
                .HasForeignKey(w => w.PersonInChargeId)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(w => w.CreatedByUser)
                .WithMany()
                .HasForeignKey(w => w.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(w => w.ModifiedByUser)
                .WithMany()
                .HasForeignKey(w => w.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(w => w.RemovedByUser)
                .WithMany()
                .HasForeignKey(w => w.RemovedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(w => w.Category)
                .WithMany(c => c.Warehouses)
                .HasForeignKey(w => w.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            //entity.HasMany(w => w.Stocks)
            //    .WithOne(s => s.Warehouse)
            //    .HasForeignKey(s => s.WarehouseId)
            //    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}