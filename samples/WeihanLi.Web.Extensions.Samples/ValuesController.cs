using System;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Services;
using WeihanLi.Web.Middleware;

namespace WeihanLi.Web.Extensions.Samples
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromServices] IUserIdProvider userIdProvider)
        {
            // var userId = User.GetUserId<int>();

            var userId2 = userIdProvider.GetUserId();

            return Ok(new object[] { userId2, new
            {
                User.Identity.IsAuthenticated,
                UserId = User.GetUserId(),
                User.Identity.Name,
                User.Identity.AuthenticationType,
            } });
        }

        [HttpGet("[action]")]
        [FeatureFlagFilter("Flag1", DefaultValue = true)]
        public IActionResult FeatureEnableTest()
        {
            return Ok(new
            {
                Time = DateTime.UtcNow
            });
        }

        [HttpGet("[action]")]
        [FeatureFlagFilter("Flag2", DefaultValue = false)]
        public IActionResult FeatureDisableTest()
        {
            return Ok(new
            {
                Time = DateTime.UtcNow
            });
        }
    }
}
