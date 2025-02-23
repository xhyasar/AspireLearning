using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Backoffice.Data.Entity;

public class Tenant : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid? ParentTenantId { get; set; }
    public string Name { get; set; } = null!;
    public string ContactName { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;

    public Tenant? ParentTenant { get; set; }
    public ICollection<Tenant>? ChildTenants { get; set; }
    public ICollection<TenantDomain>? Domains { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }

    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}

public class TenantConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasMany(t => t.Domains)
                .WithOne(d => d.Tenant)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.ParentTenant)
                .WithMany(t => t.ChildTenants)
                .HasForeignKey(t => t.ParentTenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class TenantSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasData(
            new Tenant
            {
                Id = TenantConstants.Id,
                Name = TenantConstants.Name,
                ContactName = TenantConstants.ContactName,
                ContactEmail = TenantConstants.ContactEmail,
                ContactPhone = TenantConstants.ContactPhone,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserConstants.Id,
                IsDeleted = false,
                IsActive = true
            }
        );
    }
}