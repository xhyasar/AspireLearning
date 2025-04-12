using AspireLearning.AppHost.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

if(jwtSettings is null)
    throw new InvalidConfigurationException("JwtSettings not found in configuration");

var sqlPassword = builder.AddParameter("sqlPassword", "Password12.");
var sqlServer = builder.AddSqlServer("sqlServer", sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent);

//var keycloak = builder.AddKeycloak("keycloak");

//var signalR = builder.AddAzureSignalR("signalR").RunAsEmulator();

#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosno-al-devtest-001")
    .RunAsPreviewEmulator(x =>
    {
        x.WithDataExplorer();
        x.WithLifetime(ContainerLifetime.Persistent);
    });

var cosmosDb = cosmos.AddCosmosDatabase("al-dev-001");
cosmosDb.AddContainer("Sessions", "/UserId");

var identityDb = sqlServer.AddDatabase("identityDb");
var backofficeDb = sqlServer.AddDatabase("backofficeDb");
var warehouseDb = sqlServer.AddDatabase("warehouseDb");

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var identityService = builder.AddProject<Projects.AspireLearning_Identity>("identityservice")
    .WithReference(cosmosDb).WaitFor(cosmosDb)
    .WithReference(identityDb).WaitFor(identityDb)
    .WithReference(redis).WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

var backofficeService = builder.AddProject<Projects.AspireLearning_Backoffice>("backofficeservice")
    .WithReference(cosmosDb).WaitFor(cosmosDb)
    .WithReference(backofficeDb).WaitFor(backofficeDb)
    .WithReference(redis).WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

var warehouseService = builder.AddProject<Projects.AspireLearning_Warehouse>("warehouseservice")
    .WithReference(cosmosDb).WaitFor(cosmosDb)
    .WithReference(warehouseDb).WaitFor(warehouseDb)
    .WithReference(redis).WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

var apiGateway = builder.AddProject<Projects.AspireLearning_ApiGateway>("apigateway")
    .WithReference(cosmosDb).WaitFor(cosmosDb)
    .WithReference(identityService).WaitFor(identityService)
    .WithReference(backofficeService).WaitFor(backofficeService)
    .WithReference(warehouseService).WaitFor(warehouseService);

builder.AddNpmApp("web","../AspireLearning.Front")
    .WithReference(apiGateway).WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT", port: 60000)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();