using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspireLearning.ServiceDefaults.GlobalModel.Session;
using AspireLearning.ServiceDefaults.GlobalUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable All

namespace AspireLearning.ServiceDefaults.GlobalMiddleware;

public class SessionHandlerMiddleware : IMiddleware
{
    private readonly HybridCache _cache;
    private readonly Container _container;

    public SessionHandlerMiddleware(HybridCache cache, [FromKeyedServices("Sessions")]Container container)
    {
        _cache = cache;
        _container = container;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        var language = LanguageParser.Parse(acceptLanguage);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var userId = parsedToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var session = await _cache.GetOrCreateAsync<SessionModel?>(token, async _ =>
                        await GetSessionFromCosmosAsync(userId, token), tags: ["Session"]);

                    if (session != null)
                    {
                        session.Language = language;
                        context.Items[nameof(SessionModel)] = session;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // log if needed
            }
        }
        await next(context); // sadece bir kez
    }

    private async Task<SessionModel?> GetSessionFromCosmosAsync(string userId, string token)
    {
        return await _container.GetItemLinqQueryable<SessionModel>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) })
            .FirstOrDefaultAsync(x => x.Token == token);
    }
}