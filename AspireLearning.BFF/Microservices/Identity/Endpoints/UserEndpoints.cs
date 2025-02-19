using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.BFF.Microservices.Identity.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("user/create", async ([FromServices]IIdentityClient client, UserCreateModel model) =>
        {
            await client.Register(model);
            return Results.Ok();
        })
        .WithDescription("Register")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("user", async ([FromServices]IIdentityClient client) =>
        {
            var result = await client.GetUser();
            return Results.Ok(result);
        })
        .WithDescription("Get user")
        .Produces<UserViewModel>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status400BadRequest);
    }
}