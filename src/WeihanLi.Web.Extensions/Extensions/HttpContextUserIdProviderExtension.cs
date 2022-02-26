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

public static class HttpContextUserIdProviderExtension
{
    public static IServiceCollection AddHttpContextUserIdProvider(this IServiceCollection serviceCollection)
    {
        Guard.NotNull(serviceCollection);
        serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        serviceCollection.TryAddSingleton<IUserIdProvider, HttpContextUserIdProvider>();
        return serviceCollection;
    }

    public static IServiceCollection AddHttpContextUserIdProvider(this IServiceCollection serviceCollection, Action<HttpContextUserIdProviderOptions> optionsAction)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);

        serviceCollection.Configure(optionsAction);
        return serviceCollection.AddHttpContextUserIdProvider();
    }
}
