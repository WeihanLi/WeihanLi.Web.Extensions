// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; }
    public string ApiKeyName { get; set; } = "X-ApiKey";
    public KeyLocation KeyLocation { get; set; }
    public Func<HttpContext, string, Task<bool>> ApiKeyValidator { get; set; }
    public Func<HttpContext, ApiKeyAuthenticationOptions, Claim[]> ClaimsGenerator { get; set; }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentException("Invalid ApiKey configured");
        }
    }
}

public enum KeyLocation
{
    Header = 0,
    Query = 1,
    HeaderOrQuery = 2,
    QueryOrHeader = 3,
}
