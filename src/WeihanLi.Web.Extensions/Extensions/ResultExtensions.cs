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
            ResultStatus.RequestError => new BadRequestObjectResult(result),
            ResultStatus.ResourceNotFound => new NotFoundObjectResult(result),
            ResultStatus.MethodNotAllowed => new ObjectResult(result)
            {
                StatusCode = (int)HttpStatusCode.MethodNotAllowed
            },
            ResultStatus.Unauthorized => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Unauthorized },
            ResultStatus.NoPermission => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden },
            _ => new OkObjectResult(result)
        };
    }

    public static IActionResult GetRestResult(this Result result)
    {
        return result.GetRestResult(result.Status);
    }
}
