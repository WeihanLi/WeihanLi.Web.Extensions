using Microsoft.AspNetCore.Http;
using System.Threading;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Services;

public class HttpContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual CancellationToken GetCancellationToken()
    {
        return _httpContextAccessor.HttpContext?.RequestAborted ?? default;
    }
}
