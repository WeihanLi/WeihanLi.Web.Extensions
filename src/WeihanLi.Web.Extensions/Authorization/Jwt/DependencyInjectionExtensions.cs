// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        serviceCollection.TryAddSingleton<ITokenService, JsonWebTokenService>();
        serviceCollection.ConfigureOptions<JwtTokenOptionsSetup>();
        return serviceCollection;
    }

    public static IServiceCollection AddJwtTokenServiceWithJwtBearerAuth(this IServiceCollection serviceCollection, Action<JwtTokenOptions> optionsAction, Action<JwtBearerOptions> jwtBearerOptionsSetup = null)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);
        if (jwtBearerOptionsSetup is not null)
        {
            serviceCollection.Configure(jwtBearerOptionsSetup);
        }
        serviceCollection.ConfigureOptions<JwtBearerOptionsPostSetup>();
        return serviceCollection.AddJwtTokenService(optionsAction);
    }
}
