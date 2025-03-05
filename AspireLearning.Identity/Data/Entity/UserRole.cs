using AspireLearning.ServiceDefaults.GlobalAttribute;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Identity.Data.Entity;

public class UserRole : IdentityUserRole<Guid>;

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