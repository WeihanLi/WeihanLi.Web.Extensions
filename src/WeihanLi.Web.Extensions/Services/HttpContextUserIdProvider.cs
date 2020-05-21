using System;
using Microsoft.AspNetCore.Http;
using WeihanLi.Common.Services;
using WeihanLi.Web.Extensions;

namespace WeihanLi.Web.Services
{
    public sealed class HttpContextUserIdProvider : IUserIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Func<HttpContext, string> _userIdFactory;

        public HttpContextUserIdProvider(
            IHttpContextAccessor httpContextAccessor
        ) : this(httpContextAccessor, null)
        { }

        public HttpContextUserIdProvider(
            IHttpContextAccessor httpContextAccessor,
            Func<HttpContext, string> userIdFactory
            )
        {
            _httpContextAccessor = httpContextAccessor;

            _userIdFactory = userIdFactory ?? (context => context?.User?.GetUserId());
        }

        public string GetUserId()
        {
            return _userIdFactory.Invoke(_httpContextAccessor.HttpContext);
        }
    }
}
