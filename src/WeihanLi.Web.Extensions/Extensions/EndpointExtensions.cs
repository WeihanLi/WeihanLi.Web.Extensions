// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common;
using WeihanLi.Common.Helpers;

namespace WeihanLi.Web.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapRuntimeInfo(this IEndpointRouteBuilder endpointRouteBuilder, string path = "/runtime-info")
    {
        Guard.NotNull(endpointRouteBuilder);
        endpointRouteBuilder.MapGet(path, () => ApplicationHelper.RuntimeInfo);
        return endpointRouteBuilder;
    }
}
