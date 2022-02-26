using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Services;
using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authentication.QueryAuthentication;
using WeihanLi.Web.Middleware;

namespace WeihanLi.Web.Extensions.Samples
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] IUserIdProvider userIdProvider)
        {
            var headerAuthResult = await HttpContext.AuthenticateAsync(HeaderAuthenticationDefaults.AuthenticationSchema);
            var queryAuthResult = await HttpContext.AuthenticateAsync(QueryAuthenticationDefaults.AuthenticationSchema);
            var apiKeyAuthResult = await HttpContext.AuthenticateAsync(ApiKeyAuthenticationDefaults.AuthenticationSchema);

            return Ok(new
            {
                userId = userIdProvider.GetUserId(),
                defaultAuthResult = User.Identity,
                headerAuthResult = headerAuthResult.Principal?.Identity,
                queryAuthResult = queryAuthResult.Principal?.Identity,
                apiKeyAuthResult = apiKeyAuthResult.Principal?.Identity,
            });
        }

        [HttpGet("apiKeyTest")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationSchema)]
        public IActionResult ApiKeyAuthTest()
        {
            return Ok(User.Identity);
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
