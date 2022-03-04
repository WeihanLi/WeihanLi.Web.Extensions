// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WeihanLi.Web.Extensions;

public static class HealthCheckExtensions
{
    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder)
    {
        return UseHealthCheck(applicationBuilder, new PathString("/api/health"));
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, string path)
    {
        return UseHealthCheck(applicationBuilder, new PathString(path));
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, PathString path)
    {
        static Task DefaultHealthCheck(HttpContext context, Func<Task> next)
        {
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("healthy");
        }
        applicationBuilder.Map(path, builder => builder.Use(DefaultHealthCheck));
        return applicationBuilder;
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, string path, Func<IServiceProvider, bool> checkFunc)
    {
        return UseHealthCheck(applicationBuilder, new PathString(path), serviceProvider => Task.FromResult(checkFunc(serviceProvider)));
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, string path,
        Func<IServiceProvider, Task<bool>> checkFunc)
    {
        return UseHealthCheck(applicationBuilder, new PathString(path), checkFunc);
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, PathString path, Func<IServiceProvider, bool> checkFunc)
    {
        if (checkFunc == null)
        {
            checkFunc = serviceProvider => true;
        }
        return UseHealthCheck(applicationBuilder, path, serviceProvider => Task.FromResult(checkFunc(serviceProvider)));
    }

    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder applicationBuilder, PathString path, Func<IServiceProvider, Task<bool>> checkFunc)
    {
        if (checkFunc == null)
        {
            checkFunc = serviceProvider => Task.FromResult(true);
        }
        async Task func(HttpContext context, Func<Task> next)
        {
            try
            {
                var healthy = await checkFunc.Invoke(context.RequestServices);
                if (healthy)
                {
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("healthy");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await context.Response.WriteAsync("unhealthy");
                }
            }
            catch (Exception ex)
            {
                context.RequestServices.GetService<ILoggerFactory>()
                  .CreateLogger(typeof(HealthCheckExtensions)).LogError(ex, "HealthCheck Exception");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("unhealthy");
            }
        }
        applicationBuilder.Map(path, builder => builder.Use(func));
        return applicationBuilder;
    }
}
