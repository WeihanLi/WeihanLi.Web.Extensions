// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Net;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Extensions;

public static class ResultModelExtensions
{
    public static IActionResult GetOkOResult(this Result resultModel)
    {
        return resultModel is null
            ? new OkResult()
            : new OkObjectResult(resultModel);
    }

    public static IActionResult GetRestResult(this Result resultModel)
    {
        if (resultModel == null)
            return new NoContentResult();

        if (resultModel.Status == ResultStatus.RequestError)
            return new BadRequestObjectResult(resultModel);

        if (resultModel.Status == ResultStatus.ResourceNotFound)
            return new NotFoundObjectResult(resultModel);

        if (resultModel.Status == ResultStatus.MethodNotAllowed)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.MethodNotAllowed
            };

        if (resultModel.Status == ResultStatus.Unauthorized)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };

        if (resultModel.Status == ResultStatus.NoPermission)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };

        return new OkObjectResult(resultModel);
    }

    public static IActionResult GetOkOResult(this ResultModel resultModel)
    {
        return resultModel is null
            ? new OkResult()
            : new OkObjectResult(resultModel);
    }

    public static IActionResult GetRestResult(this ResultModel resultModel)
    {
        if (resultModel == null)
            return new NoContentResult();

        if (resultModel.Status == ResultStatus.RequestError)
            return new BadRequestObjectResult(resultModel);

        if (resultModel.Status == ResultStatus.ResourceNotFound)
            return new NotFoundObjectResult(resultModel);

        if (resultModel.Status == ResultStatus.MethodNotAllowed)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.MethodNotAllowed
            };

        if (resultModel.Status == ResultStatus.Unauthorized)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };

        if (resultModel.Status == ResultStatus.NoPermission)
            return new ObjectResult(resultModel)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };

        return new OkObjectResult(resultModel);
    }
}
