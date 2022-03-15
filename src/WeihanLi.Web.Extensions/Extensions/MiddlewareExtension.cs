// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Extensions;

public static class MiddlewareExtension
{
    /// <summary>
    /// UseCustomExceptionHandler
    /// </summary>
    /// <param name="applicationBuilder">applicationBuilder</param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseMiddleware<Middleware.CustomExceptionHandlerMiddleware>();
        return applicationBuilder;
    }

    /// <summary>
    /// Use middleware if feature is enabled
    /// </summary>
    public static IApplicationBuilder UseIfFeatureEnabled(this IApplicationBuilder app, Func<RequestDelegate, RequestDelegate> middleware, string featureFlagName, bool defaultValue = false)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.IsFeatureEnabled(featureFlagName, defaultValue))
        {
            app.Use(middleware);
        }
        return app;
    }

    /// <summary>
    /// Use middleware if feature is enabled
    /// </summary>
    public static IApplicationBuilder UseIfFeatureEnabled(this IApplicationBuilder app, Func<HttpContext, RequestDelegate, Task> middleware, string featureFlagName, bool defaultValue = false)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.IsFeatureEnabled(featureFlagName, defaultValue))
        {
            app.Use(middleware);
        }
        return app;
    }

    /// <summary>
    /// Use middleware if feature is enabled
    /// </summary>
    public static IApplicationBuilder UseIfFeatureEnabled(this IApplicationBuilder app, Func<HttpContext, Func<Task>, Task> middleware, string featureFlagName, bool defaultValue = false)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.IsFeatureEnabled(featureFlagName, defaultValue))
        {
            app.Use(middleware);
        }
        return app;
    }

    /// <summary>
    /// Use middleware when feature is enabled, based on UseWhen
    /// </summary>
    public static IApplicationBuilder UseWhenFeatureEnabled(
        this IApplicationBuilder app,
        Action<IApplicationBuilder> configure,
        string featureFlagName,
        bool defaultValue = false)
    {
        return app.UseWhen(context => context.RequestServices.GetRequiredService<IConfiguration>()
            .IsFeatureEnabled(featureFlagName, defaultValue), configure);
    }
}
