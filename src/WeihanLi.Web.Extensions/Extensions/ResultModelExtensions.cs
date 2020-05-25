using System.Net;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Models;

namespace WeihanLi.Web.Extensions
{
    public static class ResultModelExtensions
    {
        public static IActionResult GetActionResult(this ResultModel resultModel)
        {
            if (resultModel == null)
                return new NoContentResult();

            if (resultModel.Status == ResultStatus.RequestError)
                return new BadRequestObjectResult(resultModel);

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
}
