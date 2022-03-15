﻿// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
        return await tokenService
            .RefreshToken(refreshToken)
            .ContinueWith(r =>
                r.Result.WrapResult().GetRestResult()
            );
    }
}
