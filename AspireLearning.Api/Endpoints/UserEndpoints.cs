using AspireLearning.Api.Data.Entity;
using AspireLearning.Api.Services;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.GlobalConstant;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.Api.Endpoints;

using Data.Context;
using Data.Entity;
using Microsoft.AspNetCore.Authorization;
using ServiceDefaults.GlobalConstant;
using Services;

public static class UserEndpoints {
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("user/create", async ([FromBody] UserCreateModel model, [FromServices] UserService service) => {
                var passwordHasher = new PasswordHasher<User>();
                var passwordHash = passwordHasher.HashPassword(new User(), model.Password);

                var identityResult = await service.CreateAsync(new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                });

                var user = await service.FindByEmailAsync(model.Email);
                await service.AddToRolesAsync(user!, [RoleConstants.User.Name]);
                
                return identityResult.Succeeded ? Results.Ok() : Results.BadRequest(identityResult.Errors);
            })
            .WithTags("UserOperations")
            .WithDescription("Register to the system")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("user", async ([FromServices] UserService service, [FromServices] SessionModel session) => {
                var user = await service.FindByEmailAsync(session.User.Email);

                if (user == null)
                    return Results.NotFound();

                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TenantId = user.TenantId,
                    Roles = service.GetRolesAsync(user).Result.ToArray()
                };

                return Results.Ok(userViewModel);
            })
            .WithTags("UserOperations")
            .WithDescription("Get current user details")
            .Produces<UserViewModel>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("user/contextTest", (HttpContext context) => {
            var userBool = context.User.Identity?.IsAuthenticated;
            
            return userBool.HasValue ? Results.Ok(userBool.Value) : Results.NotFound();
        })
        .WithTags("UserOperations")
        .WithDescription("Test user context")
        .Produces<bool>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("user/contextTest/SuperAdmin",(HttpContext context) => {
                var userBool = context.User.Identity?.IsAuthenticated;
            
                return userBool.HasValue ? Results.Ok(userBool.Value) : Results.NotFound();
            })
            .WithTags("UserOperations")
            .WithDescription("Test user context")
            .RequireAuthorization("SuperAdmin")
            .Produces<bool>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("user/contextTest/SuperAdminPolicy",(HttpContext context) => {
                var userBool = context.User.Identity?.IsAuthenticated;
            
                return userBool.HasValue ? Results.Ok(userBool.Value) : Results.NotFound();
            })
            .WithTags("UserOperations")
            .WithDescription("Test user context")
            .RequireAuthorization("SuperAdminPolicy")
            .Produces<bool>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("user/contextTest/RandomPolicy",[Authorize(Permissions.Product.Add)](HttpContext context) => {
                var userBool = context.User.Identity?.IsAuthenticated;
            
                return userBool.HasValue ? Results.Ok(userBool.Value) : Results.NotFound();
            })
            .WithTags("UserOperations")
            .WithDescription("Test user context")
            .Produces<bool>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("user/all", async ([FromServices] UserService service) => {
                var users = await service.Users
                    .ToListAsync();

                if (users.Count == 0)
                    return Results.NoContent();

                var userViewModels = users.Select(user => new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = service.GetRolesAsync(user).Result.ToArray()
                }).ToList();

                return Results.Ok(userViewModels);
            })
            .WithTags("UserOperations")
            .WithDescription("Get all users")
            .Produces<List<UserViewModel>>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
