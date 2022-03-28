// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Filters;

public sealed class ApiResultFilter: Attribute, IResultFilter, IExceptionFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is not Result)
            {
                var result = new Result<object>()
                {
                    Data = objectResult.Value
                };
                if (Enum.IsDefined(typeof(ResultStatus), objectResult.StatusCode.GetValueOrDefault()))
                {
                    result.Status = (ResultStatus)objectResult.StatusCode.GetValueOrDefault();
                }
                if (result.Status == ResultStatus.None)
                {
                    result.Status = ResultStatus.Success;
                }
                objectResult.Value = result;
            }
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
}
