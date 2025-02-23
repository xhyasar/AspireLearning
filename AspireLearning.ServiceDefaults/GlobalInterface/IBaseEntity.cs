namespace AspireLearning.ServiceDefaults.GlobalInterface;

public interface IBaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
}