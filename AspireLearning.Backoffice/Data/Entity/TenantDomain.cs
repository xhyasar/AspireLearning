using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Backoffice.Data.Entity;

public class TenantDomain
{
    public Guid Id { get; set;}
    public Guid TenantId { get; set;}
    public string Domain { get; set; } = null!;

    public Tenant? Tenant { get; set;}
}

public class TenantDomainConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantDomain>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasOne(d => d.Tenant)
                  .WithMany(t => t.Domains)
                  .HasForeignKey(d => d.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class TenantDomainSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasData(
            new TenantDomain
            {
                Id = TenantConstants.DomainId,
                TenantId = TenantConstants.Id,
                Domain = TenantConstants.Domain
            }
        );
    }
}