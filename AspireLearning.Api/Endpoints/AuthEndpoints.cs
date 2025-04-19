using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspireLearning.Api.Data.Entity;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AspireLearning.Api.Endpoints;

// Auth models
public record LoginModel(string Email, string Password);
public record UserTokenModel(string Token, UserViewModel User);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("auth/login", async (
            [FromBody]LoginModel model, 
            UserManager<Data.Entity.User> userManager, 
            RoleManager<Role> roleManager, 
            HybridCache cache, 
            [FromKeyedServices("Sessions")]Container sessionContainer) =>
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
                return Results.BadRequest("User not found");
            
            var validator = await userManager.CheckPasswordAsync(user, model.Password);
            
            if (!validator)
                return Results.BadRequest("Invalid Credentials");
            
            var userRoles = await userManager.GetRolesAsync(user);
            
            // Get user permissions from roles
            var permissions = new HashSet<string>();
            foreach (var roleName in userRoles)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                    {
                        permissions.Add(claim.Value);
                    }
                }
            }
            
            var permissionsArray = permissions.ToArray();
            
            // Generate token with roles and permissions
            var token = GenerateJwtToken(user, userRoles, permissionsArray, app.Configuration);
            
            var userViewModel = new UserViewModel(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.TenantId,
                user.IsActive,
                userRoles.ToArray()
            );
            
            var userTokenModel = new UserTokenModel(token, userViewModel);
            
            // Create SessionModel with user data including permissions
            var session = new SessionModel
            {
                UserId = userViewModel.Id.ToString(),
                Token = token,
                TenantId = userViewModel.TenantId,
                User = new UserSessionModel
                {
                    Email = userViewModel.Email,
                    FirstName = userViewModel.FirstName,
                    LastName = userViewModel.LastName,
                    Roles = userViewModel.Roles,
                    Permissions = permissionsArray
                }
            };
            
            await sessionContainer.CreateItemAsync(session, new PartitionKey(session.UserId));
            
            await cache.SetAsync(token, session, tags: ["Session"]);
            
            return Results.Ok(userTokenModel);
        })
        .WithTags("Auth")
        .WithDescription("Login to the system")
        .Produces<UserTokenModel>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static string GenerateJwtToken(Data.Entity.User user, IList<string> roles, string[] permissions, IConfiguration configuration)
    {
        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        var secret = jwtSettingsSection["SecretKey"];
        var issuer = jwtSettingsSection["Issuer"];
        var audience = jwtSettingsSection["Audience"];
        
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT SecretKey is missing in configuration");
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        // Add permission claims
        claims.AddRange(permissions.Select(permission => new Claim("Permission", permission)));
        
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}