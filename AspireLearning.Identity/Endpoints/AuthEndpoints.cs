using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AspireLearning.Identity.Data.Entity;
using AspireLearning.Identity.Services;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AspireLearning.Identity.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("auth/login", async ([FromBody]LoginModel model, UserService service, HybridCache cache) =>
        {
            var user = await service.FindByEmailAsync(model.Email);
            
            if (user == null)
                return Results.BadRequest("User not found");
            
            var validator = await service.CheckPasswordAsync(user, model.Password);
            
            if (!validator)
                return Results.BadRequest("Invalid Credentials");
            
            var roles = await service.GetRolesAsync(user);
            
            var token = GenerateJwtToken(user, roles);
            
            var userTokenModel = new UserTokenModel
            {
                Token = token,
                User = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToArray()
                }
            };
            
            var session = new SessionModel
            {
                Token = token,
                User = userTokenModel.User
            };
            
            await cache.SetAsync(token, session, tags: ["Session"]);
            
            return Results.Ok(userTokenModel);
        })
        .WithDescription("Login to the system")
        .Produces<UserTokenModel>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static string GenerateJwtToken(User user, IList<string> roles)
    {
        var secret = Environment.GetEnvironmentVariable("JwtSettings:SecretKey");
        var issuer = Environment.GetEnvironmentVariable("JwtSettings:Issuer");
        var audience = Environment.GetEnvironmentVariable("JwtSettings:Audience");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}