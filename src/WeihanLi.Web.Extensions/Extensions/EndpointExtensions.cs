// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Helpers;

namespace WeihanLi.Web.Extensions;

public static class EndpointExtensions
{
    public static IEndpointConventionBuilder MapRuntimeInfo(this IEndpointRouteBuilder endpointRouteBuilder, string path = "/runtime-info")
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);
        return endpointRouteBuilder.MapGet(path, () => ApplicationHelper.RuntimeInfo);
    }
}
