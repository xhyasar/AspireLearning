using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.GlobalModel.Identity;

namespace AspireLearning.BFF.Microservices.Identity.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("auth/login", async ([FromServices]IIdentityClient client, LoginModel model) =>
        {
            var result = await client.Login(model);
            return Results.Ok(result);
        })
        .WithDescription("Login")
        .Produces<UserTokenModel>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status400BadRequest);
    }
}