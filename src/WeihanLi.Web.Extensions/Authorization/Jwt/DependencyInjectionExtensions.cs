// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeihanLi.Common;
using WeihanLi.Web.Authorization.Token;

namespace WeihanLi.Web.Authorization.Jwt;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection, Action<JsonWebTokenOptions> optionsAction)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);
        serviceCollection.Configure(optionsAction);
        serviceCollection.TryAddSingleton<ITokenService, JsonWebTokenService>();
        serviceCollection.ConfigureOptions<JsonWebTokenOptionsSetup>();
        return serviceCollection;
    }

    public static IServiceCollection AddJwtServiceWithJwtBearerAuth(this IServiceCollection serviceCollection, Action<JsonWebTokenOptions> optionsAction, Action<JwtBearerOptions> jwtBearerOptionsSetup = null)
    {
        Guard.NotNull(serviceCollection);
        Guard.NotNull(optionsAction);
        if (jwtBearerOptionsSetup is not null)
        {
            serviceCollection.Configure(jwtBearerOptionsSetup);
        }
        serviceCollection.ConfigureOptions<JwtBearerOptionsPostSetup>();
        return serviceCollection.AddJwtService(optionsAction);
    }
}
