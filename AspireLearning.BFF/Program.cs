using AspireLearning.BFF.Handlers;
using AspireLearning.BFF.Microservices.Identity;
using AspireLearning.BFF.Microservices.Identity.Endpoints;
using AspireLearning.ServiceDefaults.GlobalMiddleware;
using Refit;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthorization();

builder.Services.AddCors(x => x.AddPolicy("AllowAll", corsPolicyBuilder => {
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddScoped<RefitHeaderHandler>();

builder.Services.AddRefitClient<IIdentityClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("services__identity__https__0")!))
    .AddHttpMessageHandler<RefitHeaderHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<SessionHandlerMiddleware>();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapIdentityEndpoints();

await app.RunAsync();

