using AspireLearning.Warehouse.Data.Context;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMicroserviceDefaults();

builder.Services.AddProblemDetails();

builder.AddSqlServerDbContext<Context>("warehouseDb");

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

app.MapDefaultEndpoints();

await app.RunAsync();