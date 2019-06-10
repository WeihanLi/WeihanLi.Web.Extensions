using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common;

namespace WeihanLi.Web.Extensions
{
    public static class DependenceResolverExtension
    {
        public static TService ResolveCustomService<TService>([NotNull]this IDependencyResolver dependencyResolver)
        {
            var contextAccessor = dependencyResolver.GetService<IHttpContextAccessor>();
            if (contextAccessor != null)
            {
                return contextAccessor.HttpContext.RequestServices.GetService<TService>();
            }
            return dependencyResolver.GetService<TService>();
        }
    }
}
