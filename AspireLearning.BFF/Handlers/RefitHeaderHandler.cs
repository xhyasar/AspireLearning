using System.Net.Http.Headers;

namespace AspireLearning.BFF.Handlers;

public class RefitHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(token))
            return await base.SendAsync(request, cancellationToken);
        
        var baseToken = token.Replace("Bearer ", "");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", baseToken);

        return await base.SendAsync(request, cancellationToken);
    }
}