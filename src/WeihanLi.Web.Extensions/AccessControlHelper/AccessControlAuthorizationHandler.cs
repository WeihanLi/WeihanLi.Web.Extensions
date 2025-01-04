// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization;
using WeihanLi.Common;

namespace WeihanLi.Web.AccessControlHelper;

internal sealed class AccessControlAuthorizationHandler(
    IHttpContextAccessor contextAccessor,
    IOptions<AccessControlOptions> options)
    : AuthorizationHandler<AccessControlRequirement>
{
    private readonly AccessControlOptions _options = options.Value;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
    {
        var httpContext = contextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(httpContext);
        var accessKey = _options.AccessKeyResolver.Invoke(httpContext);
        var resourceAccessStrategy = Guard.NotNull(httpContext).RequestServices.GetRequiredService<IResourceAccessStrategy>();
        if (resourceAccessStrategy.IsCanAccess(accessKey))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        return Task.CompletedTask;
    }
}
