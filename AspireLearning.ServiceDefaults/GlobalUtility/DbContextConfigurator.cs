using System.Reflection;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.ServiceDefaults.GlobalUtility;

public static class DbContextConfigurator
{
    public static void ConfigureDbContext(this ModelBuilder builder)
    {
        var configuratorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEntityConfigurator).IsAssignableFrom(t)
                        && t is { IsClass: true, IsAbstract: false });

        foreach (var type in configuratorTypes)
            if (Activator.CreateInstance(type) is IEntityConfigurator configurator)
                configurator.Configure(builder);

        var seederTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEntitySeeder).IsAssignableFrom(t)
                        && t is { IsClass: true, IsAbstract: false });
        foreach (var type in seederTypes)
            if (Activator.CreateInstance(type) is IEntitySeeder seeder)
                seeder.Seed(builder);
    }
}