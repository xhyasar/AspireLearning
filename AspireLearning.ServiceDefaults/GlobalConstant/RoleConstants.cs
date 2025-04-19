namespace Microsoft.Extensions.Hosting.GlobalConstant;

public static class RoleConstants
{
    public static class SuperAdmin
    {
        public static readonly Guid Id = new("1E3C5E23-1F85-45C7-8612-3C3C921CA0BE");
        public const string Name = "SuperAdmin";
    }
    
    public static class TenantAdmin
    {
        public static readonly Guid Id = new("8B5C5E29-1F65-48C7-8512-2C1C921CD0FD");
        public const string Name = "TenantAdmin";
    }
    
    public static class Admin
    {
        public static readonly Guid Id = new("f33f8da2-690d-4475-8e41-b3a9ab126ed9");
        public const string Name = "Admin";
    } 
    
    public static class User
    {
        public static readonly Guid Id = new("467250a1-b716-4be9-8068-57e46e340efe");
        public const string Name = "User";
    }
    
    public static readonly string[] DefaultRoles = { SuperAdmin.Name, TenantAdmin.Name, Admin.Name, User.Name };
}