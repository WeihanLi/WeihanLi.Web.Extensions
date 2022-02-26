﻿
/* Unmerged change from project 'WeihanLi.Web.Extensions(net5.0)'
Before:
using System.Net;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Net;
*/

/* Unmerged change from project 'WeihanLi.Web.Extensions(netcoreapp3.1)'
Before:
using System.Net;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Net;
*/
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Net;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Extensions;

public static class ResultModelExtensions
{
    public static IActionResult GetOkObjectResult(this ResultModel resultModel)
    {
        if (resultModel == null)
            return new OkResult();

        return new OkObjectResult(resultModel);
    }

    public static IActionResult GetActionResult(this ResultModel resultModel)
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
