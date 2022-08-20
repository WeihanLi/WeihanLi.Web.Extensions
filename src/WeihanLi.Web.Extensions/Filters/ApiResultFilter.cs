// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiResultFilter : Attribute
    , IResultFilter, IExceptionFilter
#if NET7_0_OR_GREATER
    , IEndpointFilter
#endif

{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: not Result } objectResult)
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
        context.Result = new ObjectResult(result) { StatusCode = 500 };
    }
#if NET7_0_OR_GREATER

    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            var result = await next(context);
            if (result is Result or ObjectResult { Value: Result } or IValueHttpResult { Value: Result })
            {
                return result;
            }

            if (result is ObjectResult { Value: not Result } objectResult)
            {
                return new Result<object>()
                {
                    Data = objectResult.Value, Status = HttpStatusCode2ResultStatus(objectResult.StatusCode)
                };
            }

            if (result is IValueHttpResult { Value: not Result } valueHttpResult)
            {
                var status = valueHttpResult is IStatusCodeHttpResult statusCodeHttpResult
                    ? HttpStatusCode2ResultStatus(statusCodeHttpResult.StatusCode)
                    : HttpStatusCode2ResultStatus(200);
                return new Result<object>() { Data = valueHttpResult.Value, Status = status };
            }

            return new Result<object>()
            {
                Data = result, Status = HttpStatusCode2ResultStatus(context.HttpContext.Response.StatusCode)
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
        var status = ResultStatus.Success;
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
