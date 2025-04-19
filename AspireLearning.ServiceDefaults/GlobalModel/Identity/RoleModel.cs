namespace Microsoft.Extensions.Hosting.GlobalModel.Identity;

public class RoleModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string[] Permissions { get; set; } = [];
}

public class RoleCreateModel
{
    public string Name { get; set; } = null!;
    
    public string[] Permissions { get; set; } = [];
}

public class RoleUpdateModel
{
    public string Name { get; set; } = null!;
    
    public string[] Permissions { get; set; } = [];
}