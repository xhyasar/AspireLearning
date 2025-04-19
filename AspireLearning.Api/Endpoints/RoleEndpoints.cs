using System.Security.Claims;
using AspireLearning.Api.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalModel.Session;

namespace AspireLearning.Api.Endpoints;

// Role related models as records
public record RoleModel(Guid Id, string Name, string[] Permissions);
public record RoleCreateModel(string Name, string[] Permissions);
public record RoleUpdateModel(string Name, string[] Permissions);

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this WebApplication app)
    {
        // Tüm rolleri getir
        app.MapGet("roles", [Authorize(Policy = "TenantAdmin")] 
                async (
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) => 
                {
                    var tenantId = session.TenantId;
                    
                    var roles = await roleManager.Roles
                        .Where(r => r.TenantId == tenantId)
                        .ToListAsync();

                    var result = new List<RoleModel>();
                    
                    foreach (var role in roles)
                    {
                        var roleModel = await GetRoleModelAsync(roleManager, role);
                        result.Add(roleModel);
                    }

                    return result;
                })
            .WithTags("RoleOperations")
            .WithDescription("Get all roles for the current tenant")
            .Produces<List<RoleModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Belirli bir rolü getir
        app.MapGet("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromRoute] Guid id, 
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var role = await roleManager.Roles
                        .Where(r => r.Id == id && r.TenantId == tenantId)
                        .FirstOrDefaultAsync();

                    if (role == null)
                    {
                        return Results.NotFound();
                    }
                    
                    var roleModel = await GetRoleModelAsync(roleManager, role);
                    return Results.Ok(roleModel);
                })
            .WithTags("RoleOperations")
            .WithDescription("Get role by id")
            .Produces<RoleModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Yeni rol oluştur
        app.MapPost("roles", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromBody] RoleCreateModel model,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    // Rol adının benzersiz olduğunu kontrol et (tenant içinde)
                    var existingRole = await roleManager.Roles
                        .Where(r => r.TenantId == tenantId && r.Name == model.Name)
                        .FirstOrDefaultAsync();
                    
                    if (existingRole != null)
                    {
                        return Results.BadRequest(new[] { new { Code = "DuplicateRoleName", Description = "Bu rol adı zaten kullanılıyor." } });
                    }

                    var role = new Role(model.Name)
                    {
                        TenantId = tenantId
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        return Results.BadRequest(result.Errors);
                    }

                    // Rol oluşturuldu, şimdi izinleri ekleyelim
                    foreach (var permission in model.Permissions)
                    {
                        await AddPermissionToRoleAsync(roleManager, role, permission);
                    }

                    return Results.Ok();
                })
            .WithTags("RoleOperations")
            .WithDescription("Create a new role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Rolü güncelle
        app.MapPut("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromRoute] Guid id,
                    [FromBody] RoleUpdateModel model,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var role = await roleManager.Roles
                        .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
                    
                    if (role == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "RoleNotFound", Description = "Rol bulunamadı." } });
                    }

                    // Rolün adını güncelle
                    role.Name = model.Name;
                    role.NormalizedName = model.Name.ToUpper();

                    var result = await roleManager.UpdateAsync(role);
                    if (!result.Succeeded)
                    {
                        return Results.BadRequest(result.Errors);
                    }

                    // Mevcut tüm izinleri temizle
                    var existingClaims = await roleManager.GetClaimsAsync(role);
                    foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
                    {
                        await roleManager.RemoveClaimAsync(role, claim);
                    }

                    // Yeni izinleri ekle
                    foreach (var permission in model.Permissions)
                    {
                        await AddPermissionToRoleAsync(roleManager, role, permission);
                    }

                    return Results.Ok();
                })
            .WithTags("RoleOperations")
            .WithDescription("Update an existing role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Rolü sil
        app.MapDelete("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromRoute] Guid id,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var role = await roleManager.Roles
                        .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
                    
                    if (role == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "RoleNotFound", Description = "Rol bulunamadı." } });
                    }

                    var result = await roleManager.DeleteAsync(role);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Delete a role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcıya rol atama
        app.MapPost("users/{userId}/roles/{roleId}", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromRoute] Guid userId,
                    [FromRoute] Guid roleId,
                    [FromServices] UserManager<User> userManager,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var user = await userManager.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
                    
                    if (user == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "UserNotFound", Description = "Kullanıcı bulunamadı." } });
                    }

                    var role = await roleManager.Roles
                        .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
                    
                    if (role == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "RoleNotFound", Description = "Rol bulunamadı." } });
                    }

                    var result = await userManager.AddToRoleAsync(user, role.Name!);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Assign a role to a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcıdan rol kaldırma
        app.MapDelete("users/{userId}/roles/{roleId}", [Authorize(Policy = "TenantAdmin")]
                async (
                    [FromRoute] Guid userId,
                    [FromRoute] Guid roleId,
                    [FromServices] UserManager<User> userManager,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var user = await userManager.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
                    
                    if (user == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "UserNotFound", Description = "Kullanıcı bulunamadı." } });
                    }

                    var role = await roleManager.Roles
                        .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId);
                    
                    if (role == null)
                    {
                        return Results.BadRequest(new[] { new { Code = "RoleNotFound", Description = "Rol bulunamadı." } });
                    }

                    var result = await userManager.RemoveFromRoleAsync(user, role.Name!);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Remove a role from a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Tüm izinleri listeleme
        app.MapGet("permissions", [Authorize(Policy = "TenantAdmin")]
                () => Permissions.AllPermissions)
            .WithTags("RoleOperations")
            .WithDescription("Get all available permissions")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcının izinlerini getirme
        app.MapGet("users/{userId}/permissions", [Authorize]
                async (
                    [FromRoute] Guid userId,
                    [FromServices] UserManager<User> userManager,
                    [FromServices] RoleManager<Role> roleManager,
                    [FromServices] SessionModel session) =>
                {
                    var tenantId = session.TenantId;
                    
                    var user = await userManager.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
                    
                    if (user == null)
                    {
                        return Results.Ok(new List<string>());
                    }

                    // Kullanıcının rollerini al
                    var roles = await userManager.GetRolesAsync(user);
                    var permissions = new HashSet<string>();

                    // Her rolün izinlerini getir
                    foreach (var roleName in roles)
                    {
                        var role = await roleManager.FindByNameAsync(roleName);
                        if (role != null)
                        {
                            var claims = await roleManager.GetClaimsAsync(role);
                            foreach (var claim in claims.Where(c => c.Type == "Permission"))
                            {
                                permissions.Add(claim.Value);
                            }
                        }
                    }

                    return Results.Ok(permissions.ToList());
                })
            .WithTags("RoleOperations")
            .WithDescription("Get all permissions for a user")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
    
    // Helper methods
    private static async Task<RoleModel> GetRoleModelAsync(RoleManager<Role> roleManager, Role role)
    {
        var claims = await roleManager.GetClaimsAsync(role);
        var permissions = claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToArray();
            
        return new RoleModel(role.Id, role.Name!, permissions);
    }
    
    private static async Task<IdentityResult> AddPermissionToRoleAsync(RoleManager<Role> roleManager, Role role, string permission)
    {
        var claim = new Claim("Permission", permission);
        return await roleManager.AddClaimAsync(role, claim);
    }
} 