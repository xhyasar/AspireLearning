namespace Microsoft.Extensions.Hosting.GlobalConstant;

public static class RoleConstants
{
    public static class Admin
    {
        public static readonly Guid Id = new Guid("f33f8da2-690d-4475-8e41-b3a9ab126ed9");
        public const string Name = "Admin";
    } 
    
    public static class User
    {
        public static readonly Guid Id = new Guid("467250a1-b716-4be9-8068-57e46e340efe");
        public const string Name = "User";
    }
}