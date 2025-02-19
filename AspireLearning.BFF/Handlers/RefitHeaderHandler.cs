using System.Net.Http.Headers;

namespace AspireLearning.BFF.Handlers;

public class RefitHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue(token!);

        return await base.SendAsync(request, cancellationToken);
    }
}