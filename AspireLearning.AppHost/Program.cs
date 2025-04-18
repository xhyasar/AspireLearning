using AspireLearning.AppHost.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

if(jwtSettings is null)
    throw new InvalidConfigurationException("JwtSettings not found in configuration");

var sqlPassword = builder.AddParameter("sqlPassword", "Password12.");
var sqlServer = builder.AddSqlServer("al-sql-001", sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent);

#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosno-al-devtest-001")
    .RunAsPreviewEmulator(x =>
    {
        x.WithDataExplorer();
        x.WithLifetime(ContainerLifetime.Persistent);
    });

var cosmosDb = cosmos.AddCosmosDatabase("cosno-al-dev-001");
cosmosDb.AddContainer("Sessions", "/UserId");

var sqlDatabase = sqlServer.AddDatabase("al-dev-001");

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var api = builder.AddProject<Projects.AspireLearning_Api>("api")
    .WithReference(cosmosDb).WaitFor(cosmosDb)
    .WithReference(sqlDatabase).WaitFor(sqlDatabase)
    .WithReference(redis).WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

builder.AddNpmApp("web","../AspireLearning.Front")
    .WithReference(api).WaitFor(api)
    .WithHttpEndpoint(env: "PORT", port: 60000)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();