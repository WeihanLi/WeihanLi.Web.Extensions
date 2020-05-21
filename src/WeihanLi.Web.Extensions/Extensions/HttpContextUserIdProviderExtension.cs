using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeihanLi.Common.Services;
using WeihanLi.Web.Services;

namespace WeihanLi.Web.Extensions
{
    public static class HttpContextUserIdProviderExtension
    {
        public static IServiceCollection AddHttpContextUserIdProvider(this IServiceCollection serviceCollection)
        {
            if (null == serviceCollection)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.TryAddSingleton<IUserIdProvider, HttpContextUserIdProvider>();
            return serviceCollection;
        }

        public static IServiceCollection AddHttpContextUserIdProvider(this IServiceCollection serviceCollection, Action<HttpContextUserIdProviderOptions> optionsAction)
        {
            if (null == serviceCollection)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            if (null == optionsAction)
            {
                throw new ArgumentNullException(nameof(optionsAction));
            }

            return serviceCollection.AddHttpContextUserIdProvider();
        }
    }
}
