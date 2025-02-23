using System.Text.Json;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware(IDistributedCache cache) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
        var language = LanguageParser.Parse(acceptLanguage);

        if (string.IsNullOrEmpty(token))
        {
            await next(context);
            return;
        }

        var session = await cache.GetStringAsync(token);
        
        if (string.IsNullOrEmpty(session))
        {
            // Consider adding logging here
            context.Request.Headers.Authorization = string.Empty;
            await next(context);
            return;
        }
        
        var sessionModel = JsonSerializer.Deserialize<SessionModel>(session);
        if (sessionModel != null)
        {
            sessionModel.Language = language;
            context.Items[nameof(SessionModel)] = sessionModel;
        }
        else
        {
            // Handle the unlikely scenario where sessionModel is null after deserialization
            context.Request.Headers.Authorization = string.Empty;
        }
        
        await next(context);
    }
}