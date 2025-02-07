namespace Microsoft.Extensions.Hosting.GlobalModel.Identity;

public class LoginModel
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}

public class UserTokenModel
{
    public string Token { get; set; } = null!;
    
    public UserViewModel User { get; set; } = null!;
    
    public string[] Roles { get; set; } = [];
}