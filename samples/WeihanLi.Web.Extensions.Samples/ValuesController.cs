// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;
using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authentication.QueryAuthentication;
using WeihanLi.Web.Authorization.Token;
using WeihanLi.Web.Filters;

namespace WeihanLi.Web.Extensions.Samples;

[Route("api/values")]
[ApiController]
[ApiResultFilter]
public class ValuesController : ControllerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ValuesController(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

    [HttpGet("[action]")]
    public IActionResult ServiceScopeTest()
    {
        Task.Run(() =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            Console.WriteLine(tokenService.GetHashCode());
        });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromServices] IUserIdProvider userIdProvider)
    {
        var headerAuthResult = await HttpContext.AuthenticateAsync(HeaderAuthenticationDefaults.AuthenticationSchema);
        var queryAuthResult = await HttpContext.AuthenticateAsync(QueryAuthenticationDefaults.AuthenticationSchema);
        var apiKeyAuthResult = await HttpContext.AuthenticateAsync(ApiKeyAuthenticationDefaults.AuthenticationSchema);
        var bearerAuthResult = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

        return Ok(new
        {
            userId = userIdProvider.GetUserId(),
            defaultAuthResult = User.Identity,
            headerAuthResult = headerAuthResult.Principal?.Identity,
            queryAuthResult = queryAuthResult.Principal?.Identity,
            apiKeyAuthResult = apiKeyAuthResult.Principal?.Identity,
            bearerAuthResult = bearerAuthResult.Principal?.Identity
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

    [HttpGet("getToken")]
    public async Task<IActionResult> GetToken([Required] string userName, [FromServices] ITokenService tokenService)
    {
        var token = await tokenService
            .GenerateToken(new Claim("name", userName));
        if (token is TokenEntityWithRefreshToken tokenEntityWithRefreshToken)
        {
            return tokenEntityWithRefreshToken.WrapResult().GetRestResult();
        }
        return token.WrapResult().GetRestResult();
    }

    [HttpGet("validateToken")]
    public async Task<IActionResult> ValidateToken(string token, [FromServices] ITokenService tokenService)
    {
        return await tokenService
            .ValidateToken(token)
            .ContinueWith(r =>
                r.Result.WrapResult().GetRestResult()
            );
    }

    [HttpGet("RefreshToken")]
    public async Task<IActionResult> RefreshToken(string refreshToken, [FromServices] ITokenService tokenService)
    {
        var token = await tokenService
            .RefreshToken(refreshToken);
        if (token is TokenEntityWithRefreshToken tokenEntityWithRefreshToken)
        {
            return tokenEntityWithRefreshToken.WrapResult().GetRestResult();
        }
        return token.WrapResult().GetRestResult();
    }

    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult BearerAuthTest()
    {
        return Ok();
    }

    [HttpGet("[action]")]
    public IActionResult Test()
    {
        return Ok(new { Name = "Amazing .NET" });
    }

    [HttpGet("[action]")]
    public IActionResult ExceptionTest()
    {
        throw new NotImplementedException();
    }

    [HttpGet("EnvironmentFilterTest/Dev")]
    [EnvironmentFilter("Development")]
    //[EnvironmentFilter("Production")]
    public IActionResult EnvironmentFilterDevTest()
    {
        return Ok(new { Title = ".NET is amazing!" });
    }

    [HttpGet("EnvironmentFilterTest/Prod")]
    [EnvironmentFilter("Production")]
    public IActionResult EnvironmentFilterProdTest()
    {
        return Ok(new { Title = ".NET is amazing!" });
    }

    [HttpPost("RawTextFormatterTest")]
    [Consumes("text/plain")]
    public object RawTextFormatterTest([FromBody]string input)
    {
        return new { input };
    }
}
