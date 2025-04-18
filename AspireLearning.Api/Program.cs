using AspireLearning.Api.Data.Context;
using AspireLearning.Api.Data.Entity;
using AspireLearning.Api.Endpoints;
using AspireLearning.Api.Services;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting.GlobalConstant;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMicroserviceDefaults();

builder.Services.AddProblemDetails();

builder.AddSqlServerDbContext<Context>("al-dev-001");

builder.Services.AddIdentity<User,Role>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

builder.Services.AddRouting();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("Admin"));

builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHsts();
app.UseHttpsRedirection();

app.UseRouting();
app.UsePathBase("/api");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    await context.Database.EnsureCreatedAsync();
}

app.UseAuthentication();
app.UseMiddleware<SessionHandlerMiddleware>();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapWarehouseEndpoints();

await app.RunAsync();