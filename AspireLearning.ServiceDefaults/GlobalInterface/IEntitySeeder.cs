using Microsoft.EntityFrameworkCore;

namespace AspireLearning.ServiceDefaults.GlobalInterface;

public interface IEntitySeeder
{
    void Seed(ModelBuilder modelBuilder);
}