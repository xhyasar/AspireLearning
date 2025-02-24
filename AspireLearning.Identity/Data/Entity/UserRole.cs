using AspireLearning.ServiceDefaults.GlobalAttribute;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Identity.Data.Entity;

public class UserRole : IdentityUserRole<Guid>
{
    public User User { get; set; } = null!;
    
    public Role Role { get; set; } = null!;
}

public class UserRoleConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

[SeedAfter(nameof(RoleSeeder))]
[SeedAfter(nameof(UserSeeder))]
public class UserRoleSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                UserId = UserConstants.Id,
                RoleId = RoleConstants.Admin.Id
            }
        );
    }
}