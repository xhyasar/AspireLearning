using System.ComponentModel.DataAnnotations;
using AspireLearning.ServiceDefaults.GlobalAttribute;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Identity.Data.Entity;

public class User : IdentityUser<Guid>, IBaseEntity, IDeletableEntity, ITrackableEntity
{
    public Guid TenantId { get; set; }
    
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string FirstName { get; set; } = null!;
    
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<UserRole>? UserRoles { get; set; }
}

public class UserSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var passwordHasher = new PasswordHasher<User>();
        var password = passwordHasher.HashPassword(new User(), UserConstants.Password);
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = UserConstants.Id,
                TenantId = TenantConstants.Id,
                UserName = UserConstants.Email,
                NormalizedUserName = UserConstants.Email.ToUpper(),
                Email = UserConstants.Email,
                NormalizedEmail = UserConstants.Email.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = password,
                PhoneNumber = UserConstants.PhoneNumber,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                FirstName = UserConstants.FirstName,
                LastName = UserConstants.LastName,
                CreatedAt = new DateTime(2025, 1, 1),
                CreatedBy = UserConstants.Id,
                IsDeleted = false,
                IsActive = true,
            });
        
        
    }
}