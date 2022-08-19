// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common;

namespace WeihanLi.Web.Filters;

public class ConditionalFilter : IAsyncResourceFilter
#if NET7_0_OR_GREATER
    , IEndpointFilter
#endif

{
    public Func<HttpContext, bool> ConditionFunc { get; init; } = _ => true;

    public Func<HttpContext, object> ResultFactory { get; init; } = _ =>
#if NET7_0_OR_GREATER
       Results.NotFound()
#else
            new NotFoundResult()
#endif
        ;
    public virtual async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var condition = ConditionFunc.Invoke(context.HttpContext);
        if (condition)
        {
            await next();
        }
        else
        {
            var result = ResultFactory.Invoke(context.HttpContext);
            context.Result = result as IActionResult ?? new OkObjectResult(result);
        }
    }
#if NET7_0_OR_GREATER
    public virtual async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = ConditionFunc.Invoke(context.HttpContext);
        if (result)
        {
            return await next(context);
        }
        return ResultFactory.Invoke(context.HttpContext);
    }
#endif
}

public sealed class EnvironmentFilter : ConditionalFilter
{
    public EnvironmentFilter(params string[] environmentNames)
    {
        Guard.NotNull(environmentNames);
        var allowedEnvironments = environmentNames.ToHashSet();
        ConditionFunc = c =>
        {
            var env = c.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName;
            return allowedEnvironments.Contains(env);
        };
    }
}

public sealed class NonProductionFilter : ConditionalFilter
{
    public NonProductionFilter()
    {
        ConditionFunc = c => c.RequestServices.GetRequiredService<IWebHostEnvironment>()
            .IsProduction() == false;
    }
}
