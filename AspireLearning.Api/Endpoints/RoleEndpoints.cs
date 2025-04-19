using AspireLearning.Api.Services;
using AspireLearning.ServiceDefaults.GlobalConstant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.GlobalConstant;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.Api.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this WebApplication app)
    {
        // Tüm rolleri getir
        app.MapGet("roles", [Authorize(Policy = "TenantAdmin")] 
                async ([FromServices] RoleService service) => await service.GetRolesAsync())
            .WithTags("RoleOperations")
            .WithDescription("Get all roles for the current tenant")
            .Produces<List<RoleModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Belirli bir rolü getir
        app.MapGet("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async ([FromRoute] Guid id, [FromServices] RoleService service) =>
                {
                    var role = await service.GetRoleByIdAsync(id);
                    return role != null ? Results.Ok(role) : Results.NotFound();
                })
            .WithTags("RoleOperations")
            .WithDescription("Get role by id")
            .Produces<RoleModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Yeni rol oluştur
        app.MapPost("roles", [Authorize(Policy = "TenantAdmin")]
                async ([FromBody] RoleCreateModel model, [FromServices] RoleService service) =>
                {
                    var result = await service.CreateRoleAsync(model);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Create a new role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Rolü güncelle
        app.MapPut("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async ([FromRoute] Guid id, [FromBody] RoleUpdateModel model, [FromServices] RoleService service) =>
                {
                    var result = await service.UpdateRoleAsync(id, model);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Update an existing role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Rolü sil
        app.MapDelete("roles/{id}", [Authorize(Policy = "TenantAdmin")]
                async ([FromRoute] Guid id, [FromServices] RoleService service) =>
                {
                    var result = await service.DeleteRoleAsync(id);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Delete a role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcıya rol atama
        app.MapPost("users/{userId}/roles/{roleId}", [Authorize(Policy = "TenantAdmin")]
                async ([FromRoute] Guid userId, [FromRoute] Guid roleId, [FromServices] RoleService service) =>
                {
                    var result = await service.AssignRoleToUserAsync(userId, roleId);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Assign a role to a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcıdan rol kaldırma
        app.MapDelete("users/{userId}/roles/{roleId}", [Authorize(Policy = "TenantAdmin")]
                async ([FromRoute] Guid userId, [FromRoute] Guid roleId, [FromServices] RoleService service) =>
                {
                    var result = await service.RemoveRoleFromUserAsync(userId, roleId);
                    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
                })
            .WithTags("RoleOperations")
            .WithDescription("Remove a role from a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Tüm izinleri listeleme
        app.MapGet("permissions", [Authorize(Policy = "TenantAdmin")]
                ([FromServices] RoleService service) => service.GetAllPermissions())
            .WithTags("RoleOperations")
            .WithDescription("Get all available permissions")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // Kullanıcının izinlerini getirme
        app.MapGet("users/{userId}/permissions", [Authorize]
                async ([FromRoute] Guid userId, [FromServices] RoleService service) =>
                {
                    var permissions = await service.GetUserPermissionsAsync(userId);
                    return Results.Ok(permissions);
                })
            .WithTags("RoleOperations")
            .WithDescription("Get all permissions for a user")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
} 