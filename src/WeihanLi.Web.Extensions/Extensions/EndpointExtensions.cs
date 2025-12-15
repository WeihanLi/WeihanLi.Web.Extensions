// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Helpers;
using WeihanLi.Web.Internals;
using WeihanLi.Web.Middleware;

namespace WeihanLi.Web.Extensions;

public static class EndpointExtensions
{
    public static IEndpointConventionBuilder MapRuntimeInfo(this IEndpointRouteBuilder endpointRouteBuilder, string path = "/runtime-info")
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);
        return endpointRouteBuilder.MapGet(path, () => Results.Json(ApplicationHelper.RuntimeInfo, CustomJsonContext.Default.RuntimeInfo));
    }

    public static IEndpointConventionBuilder MapConfigInspector(this IEndpointRouteBuilder endpointRouteBuilder,
        string path = "/config-inspector",
        Action<ConfigInspectorOptions>? optionsConfigure = null
        )
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        if (optionsConfigure is not null)
        {
            var options = endpointRouteBuilder.ServiceProvider.GetRequiredService<IOptions<ConfigInspectorOptions>>();
            optionsConfigure(options.Value);
        }

        return endpointRouteBuilder.MapGet($"{path}/{{configKey?}}", async (context) =>
        {
            var options = context.RequestServices.GetRequiredService<IOptions<ConfigInspectorOptions>>();
            await ConfigInspectorMiddleware.InvokeAsync(context, options);
        });
    }

    public static RouteGroupBuilder MapProbes(this IEndpointRouteBuilder endpointRouteBuilder, string prefix)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        var routeGroupBuilder = endpointRouteBuilder.MapGroup(prefix);

        routeGroupBuilder.ShortCircuit()
#if NET9_0_OR_GREATER
            .DisableHttpMetrics()
#endif
            ;

        return routeGroupBuilder;
    }
}
