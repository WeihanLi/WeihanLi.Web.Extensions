// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace WeihanLi.Web.Middleware;

public interface IFeatureFlagFilterResponseFactory
{
    public Task<IActionResult> GetResponse(ResourceExecutingContext resourceExecutingContext);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class FeatureFlagFilterAttribute : Attribute, IAsyncResourceFilter
{
    public bool DefaultValue { get; set; }
    public string FeatureFlagName { get; }
    public FeatureFlagFilterAttribute(string featureFlagName)
    {
        FeatureFlagName = featureFlagName ?? throw new ArgumentNullException(nameof(featureFlagName));
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        if (configuration.IsFeatureEnabled(FeatureFlagName, DefaultValue))
        {
            await next();
        }
        else
        {
            var responseFactory = context.HttpContext.RequestServices
                .GetService<IFeatureFlagFilterResponseFactory>();
            if (responseFactory != null)
            {
                context.Result = await responseFactory.GetResponse(context);
            }
            else
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}

public static class FeaturedMiddleware
{
    public static IApplicationBuilder UseFeaturedMiddleware<TMiddleware>(this IApplicationBuilder app, string featureFlagName, bool defaultValue = false)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.IsFeatureEnabled(featureFlagName, defaultValue))
        {
            app.UseMiddleware<TMiddleware>();
        }
        return app;
    }
}
