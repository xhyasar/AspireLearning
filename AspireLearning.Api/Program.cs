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

builder.Services.AddIdentity<User,Role>()
    .AddEntityFrameworkStores<Context>()
    .AddUserManager<UserManager<User>>()
    .AddRoleManager<RoleManager<Role>>()
    .AddDefaultTokenProviders();

builder.Services.AddRouting();

// Claims dönüşümü için servis ekle
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformation>();

// Rol ve izin tabanlı yetkilendirme politikaları
builder.Services.AddAuthorization(options =>
{
    // Rol tabanlı politikalar
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole(RoleConstants.SuperAdmin.Name));
    options.AddPolicy("TenantAdmin", policy => policy.RequireRole(RoleConstants.TenantAdmin.Name));
    options.AddPolicy("Admin", policy => policy.RequireRole(RoleConstants.Admin.Name));
    
    // İzin tabanlı politikalar - Ürün
    options.AddPolicy(Permissions.Product.Read, policy => 
        policy.RequireClaim("Permission", Permissions.Product.Read));
    options.AddPolicy(Permissions.Product.Add, policy => 
        policy.RequireClaim("Permission", Permissions.Product.Add));
    options.AddPolicy(Permissions.Product.Update, policy => 
        policy.RequireClaim("Permission", Permissions.Product.Update));
    options.AddPolicy(Permissions.Product.Delete, policy => 
        policy.RequireClaim("Permission", Permissions.Product.Delete));
    
    // İzin tabanlı politikalar - Depo
    options.AddPolicy(Permissions.Warehouse.Read, policy => 
        policy.RequireClaim("Permission", Permissions.Warehouse.Read));
    options.AddPolicy(Permissions.Warehouse.Add, policy => 
        policy.RequireClaim("Permission", Permissions.Warehouse.Add));
    options.AddPolicy(Permissions.Warehouse.Update, policy => 
        policy.RequireClaim("Permission", Permissions.Warehouse.Update));
    options.AddPolicy(Permissions.Warehouse.Delete, policy => 
        policy.RequireClaim("Permission", Permissions.Warehouse.Delete));
    
    // İzin tabanlı politikalar - Stok
    options.AddPolicy(Permissions.Stock.Read, policy => 
        policy.RequireClaim("Permission", Permissions.Stock.Read));
    options.AddPolicy(Permissions.Stock.Add, policy => 
        policy.RequireClaim("Permission", Permissions.Stock.Add));
    options.AddPolicy(Permissions.Stock.Update, policy => 
        policy.RequireClaim("Permission", Permissions.Stock.Update));
    options.AddPolicy(Permissions.Stock.Delete, policy => 
        policy.RequireClaim("Permission", Permissions.Stock.Delete));
    
    // İzin tabanlı politikalar - Kullanıcı Yönetimi
    options.AddPolicy(Permissions.UserManagement.Read, policy => 
        policy.RequireClaim("Permission", Permissions.UserManagement.Read));
    options.AddPolicy(Permissions.UserManagement.Add, policy => 
        policy.RequireClaim("Permission", Permissions.UserManagement.Add));
    options.AddPolicy(Permissions.UserManagement.Update, policy => 
        policy.RequireClaim("Permission", Permissions.UserManagement.Update));
    options.AddPolicy(Permissions.UserManagement.Delete, policy => 
        policy.RequireClaim("Permission", Permissions.UserManagement.Delete));
    
    // SuperAdmin rolleri için özel politika
    options.AddPolicy("SuperAdminPolicy", policy =>
    {
        policy.RequireAssertion(context => context.User.IsInRole(RoleConstants.SuperAdmin.Name));
    });
});

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