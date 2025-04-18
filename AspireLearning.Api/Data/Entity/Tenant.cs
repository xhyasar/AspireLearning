namespace AspireLearning.Api.Data.Entity;

using Microsoft.EntityFrameworkCore;
using ServiceDefaults.GlobalConstant;
using ServiceDefaults.GlobalInterface;

public class Tenant : IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public ICollection<User>? Users { get; set; }
    public ICollection<TenantDomain>? Domains { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public User? CreatedByUser { get; set; }
    public User? ModifiedByUser { get; set; }
    public User? RemovedByUser { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}

public class TenantConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            
            entity.HasKey(t => t.Id);
            
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasIndex(t => t.Name).IsUnique();
            
            entity.HasMany(t => t.Users)
                .WithOne(u => u.Tenant)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(t => t.Domains)
                .WithOne(td => td.Tenant)
                .HasForeignKey(td => td.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(td => td.CreatedByUser)
                .WithMany()
                .HasForeignKey(td => td.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(td => td.ModifiedByUser)
                .WithMany()
                .HasForeignKey(td => td.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasOne(td => td.RemovedByUser)
                .WithMany()
                .HasForeignKey(td => td.RemovedBy)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

public class TenantSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasData(
            new Tenant
            {
                Id = TenantConstants.Id,
                Name = TenantConstants.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            }
        );
    }
}
