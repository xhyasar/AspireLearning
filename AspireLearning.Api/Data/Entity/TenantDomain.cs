namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalAttribute;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalInterface;

public class TenantDomain {
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Domain { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }

    public User? CreatedByUser { get; set; }
    public User? ModifiedByUser { get; set; }
    public User? RemovedByUser { get; set; }

    public Tenant? Tenant { get; set; }
}

public class TenantDomainConfigurator : IEntityConfigurator 
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantDomain>(entity => {
            entity.ToTable("TenantDomains");

            entity.HasKey(td => td.Id);

            entity.Property(td => td.Domain)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(td => td.TenantId).IsUnique(false);

            entity.HasOne(td => td.Tenant)
                .WithMany(t => t.Domains)
                .HasForeignKey(td => td.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

[SeedAfter(nameof(TenantSeeder))]
public class TenantDomainSeedData : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantDomain>().HasData(
            new TenantDomain
            {
                Id = TenantConstants.DomainId,
                TenantId = TenantConstants.Id,
                Domain = TenantConstants.Domain,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserConstants.Id
            }
        );
    }
}


