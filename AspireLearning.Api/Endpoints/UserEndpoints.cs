using AspireLearning.Api.Data.Context;
using AspireLearning.Api.Data.Entity;
using AspireLearning.Api.Services;
using AspireLearning.ServiceDefaults.GlobalUtility;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspireLearning.Api.Endpoints;

public static class UserEndpoints {
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("user/create", async ([FromBody] UserCreateModel model, 
                [FromServices] UserService service,
                [FromServices] SessionModel session,
                [FromServices] Context context) => {
                // Check if email domain matches any tenant domain
                var emailDomain = model.Email.Split('@').LastOrDefault();
                if (string.IsNullOrEmpty(emailDomain))
                    return Results.BadRequest("Email.InvalidFormat");

                // Get tenant's domains
                var tenantDomains = await context.TenantDomains
                    .Where(d => d.TenantId == session.TenantId && !d.IsDeleted && d.IsActive)
                    .Select(d => d.Domain)
                    .ToListAsync();

                // Check if email domain matches any tenant domain
                bool isDomainValid = tenantDomains.Any(domain => emailDomain.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
                
                if (!isDomainValid)
                    return Results.BadRequest("Email.DomainNotAllowed");

                var passwordHasher = new PasswordHasher<User>();
                var passwordHash = passwordHasher.HashPassword(new User(), model.Password);

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    TenantId = session.TenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse(session.UserId)
                };

                var identityResult = await service.CreateAsync(user);
                if (!identityResult.Succeeded)
                    return Results.BadRequest(identityResult.Errors);

                // Add roles to user
                if (model.Roles != null && model.Roles.Any())
                    await service.AddToRolesAsync(user, model.Roles);
                
                return Results.Created($"/user/{user.Id}", null);
            })
            .WithTags("UserOperations")
            .WithDescription("Create a new user")
            .RequireAuthorization(Permissions.UserManagement.Add)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapGet("user", async ([FromServices] UserService service, [FromServices] SessionModel session) => {
                var user = await service.FindByEmailAsync(session.User.Email);

                if (user == null)
                    return Results.NotFound("User.NotFound");

                var roles = await service.GetRolesAsync(user);
                var userViewModel = new UserViewModel(
                    user.Id,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.TenantId,
                    user.IsActive,
                    roles.ToArray()
                );

                return Results.Ok(userViewModel);
            })
            .WithTags("UserOperations")
            .WithDescription("Get current user details")
            .Produces<UserViewModel>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("user/all", async (
                [FromQuery]UserQueryFilterModel query,
                [FromServices] UserService service,
                [FromServices] SessionModel session) => {
                var usersQuery = service.Users
                    .Where(u => u.TenantId == session.TenantId);

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    usersQuery = usersQuery.Where(u => 
                        u.FirstName.Contains(query.SearchTerm) || 
                        u.LastName.Contains(query.SearchTerm) || 
                        u.Email!.Contains(query.SearchTerm));
                }

                usersQuery = query.SortBy?.ToLower() switch
                {
                    "name" when query.SortDirection == "desc" => usersQuery.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName),
                    "name" => usersQuery.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                    "email" when query.SortDirection == "desc" => usersQuery.OrderByDescending(u => u.Email),
                    "email" => usersQuery.OrderBy(u => u.Email),
                    _ => usersQuery.OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                };

                var totalCount = await usersQuery.CountAsync();
                var pageNumber = query.PageNumber ?? 1;
                var pageSize = query.PageSize ?? 10;
                
                var paginatedQuery = usersQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                var users = await paginatedQuery.ToListAsync();

                if (users.Count == 0)
                    return Results.NoContent();

                var userViewModels = new List<UserViewModel>();
                
                foreach (var user in users)
                {
                    var roles = await service.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel(
                        user.Id,
                        user.Email!,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.TenantId,
                        user.IsActive,
                        roles.ToArray()
                    ));
                }

                var result = new PaginatedResult<UserViewModel>()
                {
                    Data = userViewModels,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return Results.Ok(result);
            })
            .WithTags("UserOperations")
            .WithDescription("Get all users with pagination")
            .RequireAuthorization(Permissions.UserManagement.Read)
            .Produces<PaginatedResult<UserViewModel>>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Name update (FirstName, LastName)
        app.MapPatch("user/{id}/name", async (
                Guid id,
                [FromBody] UserNameUpdateModel model,
                [FromServices] UserService service,
                [FromServices] SessionModel session) => 
            {
                var user = await service.FindByIdAsync(id.ToString());
                
                if (user == null || user.TenantId != session.TenantId)
                    return Results.NotFound("User.NotFound");

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = Guid.Parse(session.UserId);

                var result = await service.UpdateAsync(user);
                
                return result.Succeeded 
                    ? Results.Ok() 
                    : Results.BadRequest(result.Errors);
            })
            .WithTags("UserOperations")
            .WithDescription("Update user name")
            .RequireAuthorization(Permissions.UserManagement.Update)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Email update
        app.MapPatch("user/{id}/email", async (
                Guid id,
                [FromBody] UserEmailUpdateModel model,
                [FromServices] UserService service,
                [FromServices] SessionModel session) => 
            {
                var user = await service.FindByIdAsync(id.ToString());
                
                if (user == null || user.TenantId != session.TenantId)
                    return Results.NotFound("User.NotFound");

                // Check if email already exists
                var existingUser = await service.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != id)
                    return Results.BadRequest("Email.AlreadyInUse");

                user.Email = model.Email;
                user.UserName = model.Email; // Username is the same as email
                user.NormalizedEmail = model.Email.ToUpper();
                user.NormalizedUserName = model.Email.ToUpper();
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = Guid.Parse(session.UserId);

                var result = await service.UpdateAsync(user);
                
                return result.Succeeded 
                    ? Results.Ok() 
                    : Results.BadRequest(result.Errors);
            })
            .WithTags("UserOperations")
            .WithDescription("Update user email")
            .RequireAuthorization(Permissions.UserManagement.Update)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Phone number update
        app.MapPatch("user/{id}/phone", async (
                Guid id,
                [FromBody] UserPhoneUpdateModel model,
                [FromServices] UserService service,
                [FromServices] SessionModel session) => 
            {
                var user = await service.FindByIdAsync(id.ToString());
                
                if (user == null || user.TenantId != session.TenantId)
                    return Results.NotFound("User.NotFound");

                user.PhoneNumber = model.PhoneNumber;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = Guid.Parse(session.UserId);

                var result = await service.UpdateAsync(user);
                
                return result.Succeeded 
                    ? Results.Ok() 
                    : Results.BadRequest(result.Errors);
            })
            .WithTags("UserOperations")
            .WithDescription("Update user phone number")
            .RequireAuthorization(Permissions.UserManagement.Update)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Status update (active/inactive)
        app.MapPatch("user/{id}/status", async (
                Guid id,
                [FromBody] UserStatusUpdateModel model,
                [FromServices] UserService service,
                [FromServices] SessionModel session) => 
            {
                var user = await service.FindByIdAsync(id.ToString());
                
                if (user == null || user.TenantId != session.TenantId)
                    return Results.NotFound("User.NotFound");

                user.IsActive = model.IsActive;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = Guid.Parse(session.UserId);

                var result = await service.UpdateAsync(user);
                
                return result.Succeeded 
                    ? Results.Ok() 
                    : Results.BadRequest(result.Errors);
            })
            .WithTags("UserOperations")
            .WithDescription("Update user active status")
            .RequireAuthorization(Permissions.UserManagement.Update)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

// Models
public record UserCreateModel(string Email, string Password, string FirstName, string LastName, string? PhoneNumber, string[]? Roles);

public record UserViewModel(Guid Id, string Email, string FirstName, string LastName, string? PhoneNumber, Guid TenantId, bool IsActive, string[] Roles);

public record UserQueryFilterModel(string? SearchTerm, string? SortBy, string? SortDirection, int? PageNumber, int? PageSize) : IParsable<UserQueryFilterModel>
{
    public static UserQueryFilterModel Parse(string s, IFormatProvider? provider) => 
        new(null, null, null, null, null);
    
    public static bool TryParse(string? s, IFormatProvider? provider, out UserQueryFilterModel result)
    {
        result = new(null, null, null, null, null);
        return true;
    }
}

// Update models
public record UserNameUpdateModel(string FirstName, string LastName);
public record UserEmailUpdateModel(string Email);
public record UserPhoneUpdateModel(string? PhoneNumber);
public record UserStatusUpdateModel(bool IsActive);
