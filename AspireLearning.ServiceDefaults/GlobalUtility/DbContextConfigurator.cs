using System.Reflection;
using AspireLearning.ServiceDefaults.GlobalAttribute;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.ServiceDefaults.GlobalUtility;

public static class DbContextConfigurator
{
    public static void ConfigureDbContext(this ModelBuilder modelBuilder, Assembly assembly)
    {
        var configurators = assembly.GetTypes()
            .Where(t => typeof(IEntityConfigurator)
                .IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

        foreach (var type in configurators)
            if (Activator.CreateInstance(type) is IEntityConfigurator configurator)
                configurator.Configure(modelBuilder);

        // Discover Seeders
        var seederTypes = assembly.GetTypes()
            .Where(t => typeof(IEntitySeeder)
                .IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ToList();

        // Topological sort seeders based on [SeedAfter] attributes
        var sortedSeeders = TopologicalSortSeeders(seederTypes);

        // Run seeders in sorted order
        foreach (var type in sortedSeeders)
            if (Activator.CreateInstance(type) is IEntitySeeder seeder)
                seeder.Seed(modelBuilder);
    }

    private static IList<Type> TopologicalSortSeeders(IList<Type> seeders)
    {
        var sorted = new List<Type>();
        var visited = new Dictionary<Type, bool>();

        bool Visit(Type type)
        {
            if (visited.TryGetValue(type, out var inProcess))
                return !inProcess; // false if already in process (cycle detected)

            visited[type] = true;

            var dependencies = type.GetCustomAttributes<SeedAfterAttribute>()
                .Select(attr => seeders.FirstOrDefault(t => t.Name == attr.SeederName))
                .Where(t => t != null);

            foreach (var dep in dependencies)
                if (dep is not null && !Visit(dep))
                    throw new InvalidOperationException(
                        $"Circular or missing dependency detected: {type.Name} -> {dep.Name}");

            visited[type] = false;
            sorted.Add(type);
            return true;
        }

        foreach (var seeder in seeders)
            if (!visited.ContainsKey(seeder))
                if (!Visit(seeder))
                    throw new InvalidOperationException("Circular dependency detected in seeders.");

        return sorted;
    }
}