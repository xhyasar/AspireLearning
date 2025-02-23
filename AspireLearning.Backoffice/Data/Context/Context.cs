using System.Reflection;
using AspireLearning.Backoffice.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Backoffice.Data.Context;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantDomain> TenantDomains { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDbContext(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}