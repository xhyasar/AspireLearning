using Microsoft.AspNetCore.Identity;

namespace AspireLearning.Identity.Data.Entity;

public class UserRole : IdentityUserRole<Guid>
{
    public User User { get; set; } = null!;
    
    public Role Role { get; set; } = null!;
}