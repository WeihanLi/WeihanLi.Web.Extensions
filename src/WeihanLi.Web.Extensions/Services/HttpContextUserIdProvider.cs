using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using WeihanLi.Common.Services;
using WeihanLi.Web.Extensions;

namespace WeihanLi.Web.Services;

public sealed class HttpContextUserIdProviderOptions
{
    private Func<HttpContext, string> _userIdFactory = context => context?.User?.GetUserId();

    public Func<HttpContext, string> UserIdFactory
    {
        get => _userIdFactory;
        set
        {
            if (value != null)
            {
                _userIdFactory = value;
            }
        }
    }
}

public sealed class HttpContextUserIdProvider : IUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Func<HttpContext, string> _userIdFactory;

    public HttpContextUserIdProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<HttpContextUserIdProviderOptions> options
        )
    {
        _httpContextAccessor = httpContextAccessor;

        _userIdFactory = options.Value.UserIdFactory;
    }

    public string GetUserId()
    {
        return _userIdFactory.Invoke(_httpContextAccessor.HttpContext);
    }
}
