// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Helpers;
using WeihanLi.Web.Middleware;

namespace WeihanLi.Web.Extensions;

public static class EndpointExtensions
{
    public static IEndpointConventionBuilder MapRuntimeInfo(this IEndpointRouteBuilder endpointRouteBuilder, string path = "/runtime-info")
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);
        return endpointRouteBuilder.MapGet(path, () => ApplicationHelper.RuntimeInfo);
    }

    public static IEndpointConventionBuilder MapConfigInspector(this IEndpointRouteBuilder endpointRouteBuilder, string path = "/config-inspector",
         Action<ConfigInspectorOptions>? optionsConfigure = null)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);
        var app = endpointRouteBuilder.CreateApplicationBuilder();
        var pipeline = app.UseConfigInspector(Configure).Build();
        return endpointRouteBuilder.MapGet($"{path}/{{configKey?}}", pipeline);

        void Configure(ConfigInspectorOptions options)
        {
            options.Path = path;
            optionsConfigure?.Invoke(options);
        }
    }
}
