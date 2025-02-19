using AspireLearning.AppHost.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

if(jwtSettings is null)
    throw new InvalidConfigurationException("JwtSettings not found in configuration");

var sqlPassword = builder.AddParameter("sqlPassword", "Password12.");
var sqlServer = builder.AddSqlServer("sqlServer", sqlPassword);

var identityDb = sqlServer.AddDatabase("identityDb");
var backofficeDb = sqlServer.AddDatabase("backofficeDb");

var redis = builder.AddRedis("redis");

var identityService = builder.AddProject<Projects.AspireLearning_Identity>("identity")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(redis)
    .WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

var backofficeService = builder.AddProject<Projects.AspireLearning_Backoffice>("backoffice")
    .WithReference(backofficeDb)
    .WaitFor(backofficeDb)
    .WithReference(redis)
    .WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

var bffService = builder.AddProject<Projects.AspireLearning_BFF>("bff")
    .WithReference(identityService)
    .WaitFor(identityService)
    .WithReference(backofficeService)
    .WaitFor(backofficeService)
    .WithReference(redis)
    .WaitFor(redis)
    .InjectJwtSettings(jwtSettings);

builder.AddNpmApp("web","../AspireLearning.Front")
    .WithReference(bffService)
    .WaitFor(bffService)
    .WithHttpEndpoint(env: "PORT", port: 60000)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();