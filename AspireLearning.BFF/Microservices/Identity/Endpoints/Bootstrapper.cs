namespace AspireLearning.BFF.Microservices.Identity.Endpoints;

public static class Bootstrapper
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
    }
}