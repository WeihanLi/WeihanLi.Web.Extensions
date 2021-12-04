using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common;

namespace WeihanLi.Web.Extensions
{
    public static class DependenceResolverExtension
    {
        /// <summary>
        /// try get service from HttpContext.RequestServices
        /// </summary>
        /// <typeparam name="TService">TService</typeparam>
        /// <param name="dependencyResolver">dependencyResolver</param>
        /// <returns>service instance</returns>
        public static TService ResolveCurrentService<TService>(this IDependencyResolver dependencyResolver)
        {
            var contextAccessor = dependencyResolver.GetRequiredService<IHttpContextAccessor>();
            return contextAccessor.HttpContext.RequestServices.GetService<TService>();
        }
    }
}
