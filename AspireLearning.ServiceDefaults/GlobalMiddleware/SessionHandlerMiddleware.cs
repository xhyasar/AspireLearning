using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using MongoDB.Driver;
// ReSharper disable All

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware : IMiddleware
{
    private readonly HybridCache _cache;
    private readonly IMongoClient _mongoClient;

    public SessionHandlerMiddleware(HybridCache cache, IMongoClient mongoClient)
    {
        _cache = cache;
        _mongoClient = mongoClient;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        var language = LanguageParser.Parse(acceptLanguage);

        if (string.IsNullOrEmpty(token))
        {
            await next(context);
            return;
        }

        var session = await _cache.GetOrCreateAsync<SessionModel?>(token, async _ 
            => await GetSessionFromMongoAsync(token));
        
        if (session == null)
        {
            // Consider adding logging here
            context.Request.Headers.Authorization = string.Empty;
            await next(context);
            return;
        }
        
        session.Language = language;
        context.Items[nameof(SessionModel)] = session;
        
        await next(context);
    }

    private async Task<SessionModel?> GetSessionFromMongoAsync(string token)
    {
        var sessionCollection = _mongoClient
            .GetDatabase("al-dev-001")
            .GetCollection<SessionModel>("Sessions");
            
        var sessionDocument = await sessionCollection
            .Find(s => s.Token == token)
            .FirstOrDefaultAsync();
            
        return sessionDocument;
    }
}