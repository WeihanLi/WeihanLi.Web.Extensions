// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;

namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = default!;
    public string ApiKeyName { get; set; } = "X-ApiKey";
    public KeyLocation KeyLocation { get; set; }
    public Func<HttpContext, string, Task<bool>>? ApiKeyValidator { get; set; }
    public Func<HttpContext, ApiKeyAuthenticationOptions, Task<IReadOnlyCollection<Claim>>>? ClaimsGenerator { get; set; }

    public override void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);
    }
}

public enum KeyLocation
{
    Header = 0,
    Query = 1,
    HeaderOrQuery = 2,
    QueryOrHeader = 3,
}
