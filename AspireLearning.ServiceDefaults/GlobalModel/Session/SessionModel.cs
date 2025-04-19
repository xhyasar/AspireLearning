using AspireLearning.ServiceDefaults.GlobalEnum;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Newtonsoft.Json;

namespace AspireLearning.ServiceDefaults.GlobalModel.Session;

public class SessionModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; } = null!;
    
    public required Guid TenantId { get; set; }
    
    public required string Token { get; set; } = null!;
    
    public required UserSessionModel User { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    [JsonIgnore]
    public LanguageEnum Language { get; set; }
}

public class UserSessionModel
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => FirstName + " " + LastName;
    public string[] Roles { get; set; } = [];
    public string[] Permissions { get; set; } = [];
}