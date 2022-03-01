// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using WeihanLi.Common;
using WeihanLi.Common.Services;
using WeihanLi.Web.Services;

namespace WeihanLi.Web.Extensions;

public static class HttpContextTenantProviderExtension
{
    public static IServiceCollection AddHttpContextTenantProvider(this IServiceCollection serviceCollection)
    {
        Guard.NotNull(serviceCollection);
        serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        serviceCollection.TryAddSingleton<ITenantProvider, HttpContextTenantProvider>();
        return serviceCollection;
    }

    public static IServiceCollection AddHttpContextTenantProvider(this IServiceCollection serviceCollection, Action<HttpContextTenantProviderOptions> optionsAction)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);

        serviceCollection.Configure(optionsAction);
        return serviceCollection.AddHttpContextTenantProvider();
    }
}
