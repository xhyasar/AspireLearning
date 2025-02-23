using System.ComponentModel.DataAnnotations.Schema;
using AspireLearning.ServiceDefaults.GlobalEnum;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.ServiceDefaults.GlobalModel.Session;

public class SessionModel
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    
    public string Token { get; set; } = null!;
    
    public UserViewModel User { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    [NotMapped]
    public LanguageEnum Language { get; set; }
}