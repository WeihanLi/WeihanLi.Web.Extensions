// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WeihanLi.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ConditionalFilter : Attribute, IAsyncResourceFilter, IEndpointFilter

{
    public Func<HttpContext, bool> ConditionFunc { get; init; } = _ => true;

    public Func<HttpContext, object> ResultFactory { get; init; } = _ => Results.NotFound();

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
            context.Result = result switch
            {
                IActionResult actionResult => actionResult,
                IResult httpResult => new HttpResultActionResultAdapter(httpResult),
                _ => new OkObjectResult(result)
            };
        }
    }

    public virtual async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = ConditionFunc.Invoke(context.HttpContext);
        if (result)
        {
            return await next(context);
        }
        return ResultFactory.Invoke(context.HttpContext);
    }
}

internal sealed class HttpResultActionResultAdapter : IActionResult
{
    private readonly IResult _result;

    public HttpResultActionResultAdapter(IResult result)
    {
        _result = result;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        return _result.ExecuteAsync(context.HttpContext);
    }
}
