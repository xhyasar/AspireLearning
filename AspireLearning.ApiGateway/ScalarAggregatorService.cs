using System.Text.Json;

public class ScalarAggregatorService
{
    private readonly HttpClient _httpClient;
    private readonly List<ScalarServiceConfig> _services;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ScalarAggregatorService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _services = configuration.GetSection("ScalarServices").Get<List<ScalarServiceConfig>>() ?? [];
    }

    public async Task<string> GenerateMergedScalarReferenceAsync()
    {
        var mergedDocument = InitializeMergedDocument();

        foreach (var service in _services.Select(x => x.Name))
            await MergeServiceOpenApiAsync(service, mergedDocument);

        return JsonSerializer.Serialize(mergedDocument, new JsonSerializerOptions { WriteIndented = true });
    }

    private Dictionary<string, object> InitializeMergedDocument()
    {
        return new Dictionary<string, object>
        {
            ["openapi"] = "3.0.1",
            ["info"] = new Dictionary<string, object>
            {
                ["title"] = "Merged API",
                ["version"] = "1.0.0"
            },
            ["paths"] = new Dictionary<string, object>(),
            ["components"] = new Dictionary<string, object>
            {
                ["schemas"] = new Dictionary<string, object>(),
                ["securitySchemes"] = new Dictionary<string, object>()
            },
            ["servers"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["url"] = GetBaseServerUrl()
                }
            }
        };
    }

    private string GetBaseServerUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext != null
            ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
            : "http://localhost";
    }

    private async Task MergeServiceOpenApiAsync(string service, Dictionary<string, object> mergedDocument)
    {
        var serviceUrl = GetServiceUrl(service);
        if (string.IsNullOrEmpty(serviceUrl))
        {
            Console.WriteLine($"URL not found for service: {service}");
            return;
        }

        try
        {
            var response = await _httpClient.GetStringAsync($"{serviceUrl}/openapi/v1.json");
            using var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            MergeJsonSection(root, "paths", mergedDocument["paths"], $"/api/{service.Replace("service", "").ToLower()}");
            MergeJsonSection(root, "components", mergedDocument["components"], string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Scalar API for {service}: {ex.Message}");
        }
    }

    private string? GetServiceUrl(string service)
    {
        return Environment.GetEnvironmentVariable($"services__{service}__https__0")
               ?? Environment.GetEnvironmentVariable($"services__{service}__http__0");
    }

    private static void MergeJsonSection(JsonElement root, string section, object mergedSection, string prefix)
    {
        if (!root.TryGetProperty(section, out var element)) 
            return;

        var targetDict = (Dictionary<string, object>)mergedSection;

        foreach (var property in element.EnumerateObject())
            targetDict[$"{prefix}{property.Name}"] = JsonSerializer.Deserialize<object>(property.Value.GetRawText())!;
    }
}

public class ScalarServiceConfig
{
    public string Name { get; set; } = string.Empty;
}