using AspireLearning.ServiceDefaults.GlobalEnum;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Newtonsoft.Json;

namespace AspireLearning.ServiceDefaults.GlobalModel.Session;

public class SessionModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; } = null!;
    
    public required string Token { get; set; } = null!;
    
    public required UserViewModel User { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    [JsonIgnore]
    public LanguageEnum Language { get; set; }
}