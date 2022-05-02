// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Filters;

public sealed class ApiResultFilter : Attribute
    , IResultFilter, IExceptionFilter
#if NET7_0
    , IRouteHandlerFilter
#endif

{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is Result)
            return;

        if (context.Result is ObjectResult objectResult && objectResult.Value is not Result)
        {
            var result = new Result<object>()
            {
                Data = objectResult.Value,
                Status = HttpStatusCode2ResultStatus(objectResult.StatusCode)
            };
            objectResult.Value = result;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public void OnException(ExceptionContext context)
    {
        var result = Result.Fail(context.Exception.ToString(), ResultStatus.ProcessFail);
        context.Result = new ObjectResult(result)
        {
            StatusCode = 500
        };
    }
#if NET7_0

    public async ValueTask<object> InvokeAsync(RouteHandlerInvocationContext context, RouteHandlerFilterDelegate next)
    {
        try
        {
            var result = await next(context);
            if (result is Result)
            {
                return result;
            }            
            if (result is ObjectResult objectResult && objectResult.Value is not Result)
            {
                return new Result<object>()
                {
                    Data = objectResult.Value,
                    Status = HttpStatusCode2ResultStatus(objectResult.StatusCode)
                };
            }

            return new Result<object>()
            {
                Data = result,
                Status = HttpStatusCode2ResultStatus(context.HttpContext.Response.StatusCode)
            };
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.ToString(), ResultStatus.ProcessFail);
        }
    }
#endif

    private static ResultStatus HttpStatusCode2ResultStatus(int? statusCode)
    {
        statusCode ??= 200;
        ResultStatus status = ResultStatus.Success;
        if (Enum.IsDefined(typeof(ResultStatus), statusCode.Value))
        {
            status = (ResultStatus)statusCode.Value;
        }
        if (status == ResultStatus.None)
        {
            status = ResultStatus.Success;
        }
        return status;
    }
}
