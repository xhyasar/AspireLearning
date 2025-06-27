namespace AspireLearning.AppHost.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}


public static class JwtSettingInjector
{
    public static IResourceBuilder<ProjectResource> InjectJwtSettings(this IResourceBuilder<ProjectResource> builder, JwtSettings settings)
    {
        return builder.WithEnvironment("JwtSettings__SecretKey", settings.SecretKey)
            .WithEnvironment("JwtSettings__Issuer", settings.Issuer)
            .WithEnvironment("JwtSettings__Audience", settings.Audience);
    }

    public static void InjectJwtSettings(IResourceBuilder<ProjectResource>[] resources, JwtSettings settings)
    {
        foreach (var resource in resources)
        {
            resource.WithEnvironment("JwtSettings__SecretKey", settings.SecretKey)
                .WithEnvironment("JwtSettings__Issuer", settings.Issuer)
                .WithEnvironment("JwtSettings__Audience", settings.Audience);
        }
    }
}