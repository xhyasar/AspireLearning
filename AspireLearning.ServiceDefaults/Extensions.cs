using System.Text;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions {
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http => {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder AddMicroserviceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddOpenApi(opt => {
            opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
        
        builder.AddAzureCosmosDatabase("cosno-al-dev-001")
            .AddKeyedContainer("Sessions");

        builder.AddRedisDistributedCache("redis");
        
        builder.Services.AddHybridCache(o =>
        {
            o.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            };
        });
        
        builder.Services.AddScoped<SessionHandlerMiddleware>();
        
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddScoped<SessionModel>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = httpContextAccessor.HttpContext;
            return context?.Items[nameof(SessionModel)] as SessionModel ?? new SessionModel
            {
                Id = string.Empty,
                UserId = string.Empty,
                Token = string.Empty,
                User = new(),
                TenantId = default,
                CreatedAt = default,
                Language = 0
            };
        });
        
        var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettingsSection["SecretKey"];
        var issuer = jwtSettingsSection["Issuer"];
        var audience = jwtSettingsSection["Audience"];

        // JWT ayarlarının mevcut olduğunu kontrol et ve hataları logla
        if (string.IsNullOrEmpty(secretKey))
        {
            builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<ILogger>>());
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger>();
            logger.LogError("JWT SecretKey is missing in configuration");
        }
        
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.ASCII.GetBytes(secretKey ?? "DefaultSecretKeyForDevelopment-ThisIsNotSecure");
                
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Geliştirme ortamında false olabilir
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                };
                
                // Token doğrulama hatalarını loglama
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("JWT token successfully validated for: {Name}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("JWT token received: {Token}", context.Token?.Substring(0, Math.Min(10, context.Token?.Length ?? 0)));
                        return Task.CompletedTask;
                    }
                };
            });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging => {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing => {
                tracing.AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
