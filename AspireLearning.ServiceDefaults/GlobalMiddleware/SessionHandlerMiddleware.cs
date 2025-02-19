using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting.GlobalModel.Session;

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware(IDistributedCache cache) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            await next(context);
            return;
        }
        
        var session = await cache.GetStringAsync(token);
        
        if(session == null)
        {
            context.Request.Headers.Authorization = string.Empty;
            await next(context);
            return;
        }
        
        var sessionModel = JsonSerializer.Deserialize<SessionModel>(session);
        context.Items.Add(nameof(SessionModel), sessionModel);
        
        await next(context);
    }
}