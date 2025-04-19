using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Microsoft.IdentityModel.Tokens;
using Entity_User=AspireLearning.Api.Data.Entity.User;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AspireLearning.Api.Endpoints;

using Services;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("auth/login", async ([FromBody]LoginModel model, UserService service, HybridCache cache, [FromKeyedServices("Sessions")]Container sessionContainer) =>
        {
            var user = await service.FindByEmailAsync(model.Email);
            
            if (user == null)
                return Results.BadRequest("User not found");
            
            var validator = await service.CheckPasswordAsync(user, model.Password);
            
            if (!validator)
                return Results.BadRequest("Invalid Credentials");
            
            var roles = await service.GetRolesAsync(user);
            
            var token = GenerateJwtToken(user, roles, app.Configuration);
            
            var userTokenModel = new UserTokenModel
            {
                Token = token,
                User = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    TenantId = user.TenantId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToArray()
                }
            };
            
            var session = new SessionModel
            {
                UserId = userTokenModel.User.Id.ToString(),
                Token = token,
                User = userTokenModel.User,
                TenantId = userTokenModel.User.TenantId
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

    private static string GenerateJwtToken(Entity_User user, IList<string> roles, IConfiguration configuration)
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
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
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