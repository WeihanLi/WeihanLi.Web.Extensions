// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Net;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Extensions;

public static class ResultModelExtensions
{
    public static IActionResult GetOkResult<T>(this T result)
    {
        return result is null
            ? new OkResult()
            : new OkObjectResult(result);
    }

    public static IActionResult GetRestResult<T>(this T result, ResultStatus status = ResultStatus.Success)
    {
        if (result is null)
            return new NoContentResult();

        return status switch
        {
            ResultStatus.BadRequest => new BadRequestObjectResult(result),
            ResultStatus.NotFound => new NotFoundObjectResult(result),
            ResultStatus.MethodNotAllowed => new ObjectResult(result)
            {
                StatusCode = (int)HttpStatusCode.MethodNotAllowed
            },
            ResultStatus.Unauthorized => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Unauthorized },
            ResultStatus.Forbidden => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden },
            _ => new OkObjectResult(result)
        };
    }
    
    public static IActionResult GetRestResult(this ResultStatus status)
    {
        return status switch
        {
            ResultStatus.BadRequest => new BadRequestResult(),
            ResultStatus.NotFound => new NotFoundResult(),
            ResultStatus.MethodNotAllowed => new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed),
            ResultStatus.Unauthorized => new StatusCodeResult((int)HttpStatusCode.Unauthorized),
            ResultStatus.Forbidden => new StatusCodeResult((int)HttpStatusCode.Forbidden),
            ResultStatus.Success => new OkResult(),
            _ => new StatusCodeResult((int)status)
        };
    }

    public static IActionResult GetRestResult(this Result result)
    {
        return GetRestResult(result, result.Status);
    }

    public static IActionResult GetRestResult<T>(this Result<T> result)
    {
        return GetRestResult(result, result.Status);
    }
}
