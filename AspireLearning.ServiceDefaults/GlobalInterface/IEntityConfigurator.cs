using Microsoft.EntityFrameworkCore;

namespace AspireLearning.ServiceDefaults.GlobalInterface;

public interface IEntityConfigurator
{
    void Configure(ModelBuilder modelBuilder);
}
