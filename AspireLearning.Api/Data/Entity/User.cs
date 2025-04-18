using System.ComponentModel.DataAnnotations;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Data.Entity;

using Microsoft.Extensions.Hosting.GlobalConstant;
using ServiceDefaults.GlobalAttribute;

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
    
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? RemovedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Tenant? Tenant { get; set; }
}

public class UserConfigurator : IEntityConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TenantId)
            .IsUnique(false);
    }
}

[SeedAfter(nameof(TenantSeeder))]
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
                IsDeleted = false,
                IsActive = true,
            }
        );
    }
}

[SeedAfter(nameof(RoleSeeder))]
[SeedAfter(nameof(UserSeeder))]
public class UserRoleSeeder : IEntitySeeder
{
    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
            {
                UserId = UserConstants.Id,
                RoleId = RoleConstants.Admin.Id
            }
        );
    }
}