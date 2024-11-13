// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.AccessControlHelper;

/// <summary>
/// AccessControlHelperMiddleware
/// </summary>
internal sealed class AccessControlHelperMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AccessControlOptions _option;

    /// <summary>
    /// Creates a new instance of <see cref="AccessControlHelperMiddleware"/>
    /// </summary>
    /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
    /// <param name="options"></param>
    public AccessControlHelperMiddleware(
        RequestDelegate next,
        IOptions<AccessControlOptions> options
        )
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
        _option = options.Value;
    }

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the execution of this middleware.</returns>
    public Task Invoke(HttpContext context)
    {
        var accessKey = _option.AccessKeyResolver.Invoke(context);
        var accessStrategy = context.RequestServices.GetRequiredService<IResourceAccessStrategy>();
        if (accessStrategy.IsCanAccess(accessKey))
        {
            return _next(context);
        }

        context.Response.StatusCode = context.User is { Identity.IsAuthenticated: true } ? 403 : 401;
        return _option.DefaultUnauthorizedOperation?.Invoke(context) ?? Task.CompletedTask;
    }
}
