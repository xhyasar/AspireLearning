using AspireLearning.Identity.Data.Entity;
using AspireLearning.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.GlobalConstant;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.Identity.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/user/create", async ([FromBody] UserCreateModel model, [FromServices] UserService service) =>
        {
            var passwordHasher = new PasswordHasher<User>();
            var passwordHash = passwordHasher.HashPassword(null, model.Password);
            
            var identityResult = await service.CreateAsync(new User
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PasswordHash = passwordHash,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = RoleConstants.User.Id
                    }
                ]
            });
            
            return identityResult.Succeeded ? Results.Ok() : Results.BadRequest(identityResult.Errors);
        })
        .WithDescription("Register to the system")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
        
        app.MapGet("/user/{id:guid}", async ([FromRoute] Guid id, [FromServices] UserService service) =>
        {
            var user = await service.FindByIdAsync(id.ToString());
            
            if (user == null)
                return Results.NotFound();
            
            var roles = await service.GetRolesAsync(user);
            
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToArray()
            };
            
            return Results.Ok(userViewModel);
        })
        .WithDescription("Get user by id")
        .Produces<UserViewModel>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status404NotFound);
    }
}