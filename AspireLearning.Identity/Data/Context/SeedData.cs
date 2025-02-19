using AspireLearning.Identity.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;

namespace AspireLearning.Identity.Data.Context;

public static class SeedData
{
    public static void SeedRoles(this ModelBuilder builder)
    {
        builder.Entity<Role>().HasData(
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