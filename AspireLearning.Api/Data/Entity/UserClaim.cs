using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Data.Entity;

public class UserClaim : IdentityUserClaim<Guid>
{
    public UserClaim()
    {
    }
    
    // TenantId ekleyerek tenant-aware yapıyoruz
    public Guid? TenantId { get; set; }
}

public class UserClaimConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserClaim>()
            .HasIndex(x => x.UserId);
        
        modelBuilder.Entity<UserClaim>()
            .HasIndex(x => x.TenantId);
    }
} 