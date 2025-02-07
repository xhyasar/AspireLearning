var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sqlPassword", "Password12.");
var sqlServer = builder.AddSqlServer("sqlServer", sqlPassword);

var identityDb = sqlServer.AddDatabase("identityDb");
var backofficeDb = sqlServer.AddDatabase("backofficeDb");

var redis = builder.AddRedis("redis");

var identityService = builder.AddProject<Projects.AspireLearning_Identity>("identity")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(redis)
    .WaitFor(redis);

var backofficeService = builder.AddProject<Projects.AspireLearning_Backoffice>("backoffice")
    .WithReference(backofficeDb)
    .WaitFor(backofficeDb)
    .WithReference(redis)
    .WaitFor(redis);

var bffService = builder.AddProject<Projects.AspireLearning_BFF>("bff")
    .WithReference(identityService)
    .WaitFor(identityService)
    .WithReference(backofficeService)
    .WaitFor(backofficeService)
    .WithReference(redis)
    .WaitFor(redis);

builder.AddNpmApp("web","../AspireLearning.Front")
    .WithReference(bffService)
    .WaitFor(bffService)
    .WithHttpEndpoint(env: "PORT", port: 60000)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


await builder.Build().RunAsync();
