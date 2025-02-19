using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AspireLearning.Identity.Data.Entity;

public class User : IdentityUser<Guid>
{
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string FirstName { get; set; } = null!;
    
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string LastName { get; set; } = null!;
    
    public string FullName => $"{FirstName} {LastName}";
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public ICollection<UserRole> UserRoles { get; set; } = [];
}