using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Api.Data.Entity;

public class Role : IdentityRole<Guid>;

public class RoleSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = RoleConstants.Admin.Id,
                Name = RoleConstants.Admin.Name
            },
            new Role
            {
                Id = RoleConstants.User.Id,
                Name = RoleConstants.User.Name
            }
        );
    }
}