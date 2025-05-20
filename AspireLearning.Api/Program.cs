using AspireLearning.Api.Data.Context;
using AspireLearning.Api.Data.Entity;
using AspireLearning.Api.Endpoints;
using AspireLearning.Api.Services;
using AspireLearning.ServiceDefaults.GlobalConstant;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting.GlobalConstant;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMicroserviceDefaults();

builder.Services.AddProblemDetails();

builder.AddSqlServerDbContext<Context>("al-dev-001");

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<Context>()
    .AddUserManager<UserManager<User>>()
    .AddRoleManager<RoleManager<Role>>()
    .AddDefaultTokenProviders();

builder.Services.AddRouting();

// Rol ve izin tabanlı yetkilendirme politikaları
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SuperAdmin", policy => policy.RequireRole(RoleConstants.SuperAdmin.Name))
    .AddPolicy("TenantAdmin", policy => policy.RequireRole(RoleConstants.TenantAdmin.Name))
    .AddPolicy(Permissions.Product.Read, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Product.Read))
    .AddPolicy(Permissions.Product.Add, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Product.Add))
    .AddPolicy(Permissions.Product.Update, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Product.Update))
    .AddPolicy(Permissions.Product.Delete, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Product.Delete))
    .AddPolicy(Permissions.Warehouse.Read, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Warehouse.Read))
    .AddPolicy(Permissions.Warehouse.Add, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Warehouse.Add))
    .AddPolicy(Permissions.Warehouse.Update, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Warehouse.Update))
    .AddPolicy(Permissions.Warehouse.Delete, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Warehouse.Delete))
    .AddPolicy(Permissions.Stock.Read, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Stock.Read))
    .AddPolicy(Permissions.Stock.Add, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Stock.Add))
    .AddPolicy(Permissions.Stock.Update, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Stock.Update))
    .AddPolicy(Permissions.Stock.Delete, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.Stock.Delete))
    .AddPolicy(Permissions.UserManagement.Read, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.UserManagement.Read))
    .AddPolicy(Permissions.UserManagement.Add, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.UserManagement.Add))
    .AddPolicy(Permissions.UserManagement.Update, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.UserManagement.Update))
    .AddPolicy(Permissions.UserManagement.Delete, policy =>
        policy.RequireClaim(Permissions.ClaimType, Permissions.UserManagement.Delete));

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
app.MapRoleEndpoints();
app.MapTenantEndpoints();
app.MapWarehouseEndpoints();
app.MapWarehouseCategoryEndpoints();
app.MapProductEndpoints();
app.MapProductStockEndpoints();
app.MapStockTransactionEndpoints();

await app.RunAsync();