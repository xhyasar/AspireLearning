using System.Reflection;
using AspireLearning.Identity.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Identity.Data.Context;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<User, Role, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDbContext(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(builder);
    }
}