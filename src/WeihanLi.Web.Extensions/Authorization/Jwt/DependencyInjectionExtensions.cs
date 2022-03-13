// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection.Extensions;
using WeihanLi.Common;
using WeihanLi.Web.Authorization.Token;

namespace WeihanLi.Web.Authorization.Jwt;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddJwtTokenService(this IServiceCollection serviceCollection, Action<JwtTokenOptions> optionsAction)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);
        serviceCollection.Configure(optionsAction);
        serviceCollection.TryAddSingleton<ITokenService, JwtTokenService>();
        serviceCollection.ConfigureOptions<JwtTokenOptionsSetup>();
        return serviceCollection;
    }
}
