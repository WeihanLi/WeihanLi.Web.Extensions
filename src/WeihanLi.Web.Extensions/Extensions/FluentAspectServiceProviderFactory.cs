// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using System.Linq.Expressions;
using WeihanLi.Common.Aspect;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Extensions;

internal sealed class FluentAspectsServiceProviderFactory(
    Action<FluentAspectOptions> optionsAction,
    Action<IFluentAspectsBuilder>? aspectBuildAction,
    Expression<Func<Type, bool>> ignoreTypesPredict)
    : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return containerBuilder.BuildFluentAspectsProvider(optionsAction, aspectBuildAction, ignoreTypesPredict);
    }
}

public static class FluentAspectServiceProviderFactoryExtensions
{
    public static IHostBuilder UseFluentAspectsServiceProviderFactory(this IHostBuilder hostBuilder,
        Action<FluentAspectOptions> optionsAction,
        Action<IFluentAspectsBuilder>? aspectBuildAction = null,
        Expression<Func<Type, bool>>? ignoreTypesPredict = null)
    {
        if (ignoreTypesPredict == null)
        {
            ignoreTypesPredict = t =>
                t.HasNamespace()
                && (t.Namespace!.StartsWith("Microsoft.") == true
                    || t.Namespace.StartsWith("System.") == true)
                ;
        }
        hostBuilder.UseServiceProviderFactory(
            new FluentAspectsServiceProviderFactory(optionsAction, aspectBuildAction, ignoreTypesPredict)
            );
        return hostBuilder;
    }
}
