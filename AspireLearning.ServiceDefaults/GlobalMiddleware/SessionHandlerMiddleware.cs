using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
// ReSharper disable All

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware : IMiddleware
{
    private readonly HybridCache _cache;
    private readonly Container _container;

    public SessionHandlerMiddleware(HybridCache cache, CosmosClient client)
    {
        _cache = cache;
        _container = client.GetDatabase("al-dev-001").GetContainer("Sessions");
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
        
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userId = parsedToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Request.Headers.Authorization = string.Empty;
            await next(context);
            return;
        }
        
        var session = await _cache.GetOrCreateAsync<SessionModel?>(token, async _ 
            => await GetSessionFromMongoAsync(userId, token));
        
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

    private async Task<SessionModel?> GetSessionFromMongoAsync(string userId, string token)
    {
        
        return await _container.GetItemLinqQueryable<SessionModel>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) })
            .FirstOrDefaultAsync(x => x.Token == token);
    }
}