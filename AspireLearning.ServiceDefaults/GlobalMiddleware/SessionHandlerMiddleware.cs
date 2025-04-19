using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable All

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware : IMiddleware
{
    private readonly HybridCache _cache;
    private readonly Container _container;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SessionHandlerMiddleware> _logger;

    public SessionHandlerMiddleware(
        HybridCache cache, 
        [FromKeyedServices("Sessions")]Container container,
        IConfiguration configuration,
        ILogger<SessionHandlerMiddleware> logger)
    {
        _cache = cache;
        _container = container;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        var language = LanguageParser.Parse(acceptLanguage);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Tam token validasyonu gerçekleştir
                var tokenValidationResult = ValidateToken(token);
                
                if (tokenValidationResult.IsValid)
                {
                    var userId = tokenValidationResult.ClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!string.IsNullOrEmpty(userId))
                    {
                        var session = await _cache.GetOrCreateAsync<SessionModel?>(token, async _ =>
                            await GetSessionFromCosmosAsync(userId, token), tags: ["Session"]);

                        if (session != null)
                        {
                            session.Language = language;
                            
                            // Validate işleminden sonra oluşturulan ClaimsPrincipal'i HttpContext.User'a aktar
                            context.User = tokenValidationResult.ClaimsPrincipal!;
                            
                            context.Items[nameof(SessionModel)] = session;
                            _logger.LogInformation("Session successfully validated and set for user: {UserId}", userId);
                        }
                        else
                        {
                            _logger.LogWarning("Valid token but no session found for user: {UserId}", userId);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Token validation failed: {Error}", tokenValidationResult.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during token validation");
            }
        }
        
        await next(context);
    }

    private async Task<SessionModel?> GetSessionFromCosmosAsync(string userId, string token)
    {
        return await _container.GetItemLinqQueryable<SessionModel>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) })
            .FirstOrDefaultAsync(x => x.Token == token);
    }
    
    private TokenValidationResult ValidateToken(string token)
    {
        try
        {
            // JWT ayarlarını konfigürasyondan al
            var jwtSettingsSection = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettingsSection["SecretKey"];
            var issuer = jwtSettingsSection["Issuer"];
            var audience = jwtSettingsSection["Audience"];
            
            if (string.IsNullOrEmpty(secretKey))
            {
                return new TokenValidationResult { IsValid = false, ErrorMessage = "JWT SecretKey is missing in configuration" };
            }
            
            // Token doğrulama parametrelerini oluştur
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            // Token'ı doğrula
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            
            // Token'ın tipini kontrol et
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Invalid token algorithm" };
            }
            
            // Token'ın süresinin dolup dolmadığını kontrol et
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Token expired" };
            }
            
            // Tüm kontroller başarılı, geçerli bir token
            return new TokenValidationResult { IsValid = true, ClaimsPrincipal = principal };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = "Token expired" };
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = "Invalid token signature" };
        }
        catch (SecurityTokenValidationException ex)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = $"Token validation failed: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult { IsValid = false, ErrorMessage = $"Token validation error: {ex.Message}" };
        }
    }
}

// Token doğrulama sonucunu temsil eden sınıf
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public ClaimsPrincipal? ClaimsPrincipal { get; set; }
}