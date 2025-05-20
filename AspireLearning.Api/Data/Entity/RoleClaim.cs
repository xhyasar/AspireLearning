using AspireLearning.ServiceDefaults.GlobalAttribute;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;
using System.Security.Claims;

namespace AspireLearning.Api.Data.Entity;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public RoleClaim()
    {
    }
    
    // TenantId ekleyerek tenant-aware yapıyoruz
    public Guid? TenantId { get; set; }
}

public class RoleClaimConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleClaim>()
            .HasIndex(x => x.RoleId);
        
        modelBuilder.Entity<RoleClaim>()
            .HasIndex(x => x.TenantId);
    }
}

[SeedAfter(nameof(RoleSeeder))]
public class RoleClaimSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        // SuperAdmin rolü için tüm izinleri ekle
        var superAdminRoleId = RoleConstants.SuperAdmin.Id;
        var claimId = 1;
        
        foreach (var permission in Permissions.AllPermissions)
        {
            modelBuilder.Entity<RoleClaim>().HasData(
                new RoleClaim
                {
                    Id = claimId++,
                    RoleId = superAdminRoleId,
                    ClaimType = Permissions.ClaimType,
                    ClaimValue = permission
                }
            );
        }
        
        // TenantAdmin rolü için tüm izinleri ekle
        var tenantAdminRoleId = RoleConstants.TenantAdmin.Id;
        
        foreach (var permission in Permissions.AllPermissions)
        {
            modelBuilder.Entity<RoleClaim>().HasData(
                new RoleClaim
                {
                    Id = claimId++,
                    RoleId = tenantAdminRoleId,
                    ClaimType = Permissions.ClaimType,
                    ClaimValue = permission,
                    TenantId = TenantConstants.Id
                }
            );
        }
    }
} 