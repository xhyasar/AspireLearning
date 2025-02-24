using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Warehouse.Data.Context;

public class Context(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}