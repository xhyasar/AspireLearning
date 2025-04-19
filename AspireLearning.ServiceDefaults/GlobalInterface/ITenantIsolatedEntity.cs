namespace AspireLearning.ServiceDefaults.GlobalInterface;

public interface ITenantIsolatedEntity
{
    public Guid TenantId { get; set; }
}