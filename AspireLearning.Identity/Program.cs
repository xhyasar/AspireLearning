using AspireLearning.Identity.Data.Context;
using AspireLearning.Identity.Data.Entity;
using AspireLearning.Identity.Endpoints;
using AspireLearning.Identity.Services;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.AddSqlServerDbContext<Context>("identityDb");

builder.Services.AddIdentity<User,Role>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHsts();
app.UseHttpsRedirection();

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

await app.RunAsync();