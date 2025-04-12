using AspireLearning.Backoffice.Data.Context;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMicroserviceDefaults();

builder.Services.AddProblemDetails();

builder.AddSqlServerDbContext<Context>("backofficeDb");

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    await context.Database.EnsureCreatedAsync();
}

app.UseHsts();
app.UseHttpsRedirection();

app.UseMiddleware<SessionHandlerMiddleware>();

app.MapDefaultEndpoints();

await app.RunAsync();
