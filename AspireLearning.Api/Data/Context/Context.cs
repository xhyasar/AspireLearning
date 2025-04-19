using System.Reflection;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Data.Context;

using Entity;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<User, Role, Guid, UserClaim, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, RoleClaim, IdentityUserToken<Guid>>(options)
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantDomain> TenantDomains { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    
    public DbSet<Country> Countries { get; set; }
    public DbSet<CountryText> CountryTexts { get; set; }
    
    public DbSet<City> Cities { get; set; }
    public DbSet<CityText> CityTexts { get; set; }
    
    public DbSet<WarehouseCategory> WarehouseCategories { get; set; }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductStock> ProductStocks { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDbContext(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(builder);
    }
}