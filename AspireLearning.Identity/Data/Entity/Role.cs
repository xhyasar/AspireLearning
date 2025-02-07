using Microsoft.AspNetCore.Identity;

namespace AspireLearning.Identity.Data.Entity;

public class Role : IdentityRole<Guid>
{
    public UserRole[] UserRoles { get; set; } = [];
}