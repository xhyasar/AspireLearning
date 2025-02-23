using AspireLearning.Backoffice.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Backoffice.Data.Context;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants = null!;
    public DbSet<TenantDomain> TenantDomains = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDbContext();
        base.OnModelCreating(builder);
    }
}