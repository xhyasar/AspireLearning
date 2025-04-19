using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Api.Data.Entity;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
    }
    
    public Role(string roleName) : base(roleName)
    {
    }
    
    // Tenant yetkisinde olunca doldurulacak, SuperAdmin için null olabilir
    public Guid? TenantId { get; set; }
}

public class RoleConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.TenantId)
            .IsUnique(false);
    }
}

public class RoleSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        // SuperAdmin rolü
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = RoleConstants.SuperAdmin.Id,
                Name = RoleConstants.SuperAdmin.Name,
                NormalizedName = RoleConstants.SuperAdmin.Name.ToUpper(),
                TenantId = null
            },
            new Role
            {
                Id = RoleConstants.TenantAdmin.Id,
                Name = RoleConstants.TenantAdmin.Name,
                NormalizedName = RoleConstants.TenantAdmin.Name.ToUpper(),
                TenantId = TenantConstants.Id // İlk tenant için
            },
            new Role
            {
                Id = RoleConstants.Admin.Id,
                Name = RoleConstants.Admin.Name,
                NormalizedName = RoleConstants.Admin.Name.ToUpper(),
                TenantId = TenantConstants.Id
            },
            new Role
            {
                Id = RoleConstants.User.Id,
                Name = RoleConstants.User.Name,
                NormalizedName = RoleConstants.User.Name.ToUpper(),
                TenantId = TenantConstants.Id
            }
        );
    }
}