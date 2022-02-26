
/* Unmerged change from project 'WeihanLi.Web.Extensions(net5.0)'
Before:
using System;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System;
*/

/* Unmerged change from project 'WeihanLi.Web.Extensions(netcoreapp3.1)'
Before:
using System;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System;
*/
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication;
using System;

/* Unmerged change from project 'WeihanLi.Web.Extensions(net5.0)'
Before:
namespace WeihanLi.Web.Authentication.ApiKeyAuthentication
{
    public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ApiKey { get; set; }
        public string ApiKeyName { get; set; } = "X-ApiKey";
        public string ClientId { get; set; }
        public KeyLocation KeyLocation { get; set; }

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
        HeaderOrQuery,
    }
After:
namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; }
    public string ApiKeyName { get; set; } = "X-ApiKey";
    public string ClientId { get; set; }
    public KeyLocation KeyLocation { get; set; }

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
    HeaderOrQuery,
*/

/* Unmerged change from project 'WeihanLi.Web.Extensions(netcoreapp3.1)'
Before:
namespace WeihanLi.Web.Authentication.ApiKeyAuthentication
{
    public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ApiKey { get; set; }
        public string ApiKeyName { get; set; } = "X-ApiKey";
        public string ClientId { get; set; }
        public KeyLocation KeyLocation { get; set; }

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
        HeaderOrQuery,
    }
After:
namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; }
    public string ApiKeyName { get; set; } = "X-ApiKey";
    public string ClientId { get; set; }
    public KeyLocation KeyLocation { get; set; }

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
    HeaderOrQuery,
*/

namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; }
    public string ApiKeyName { get; set; } = "X-ApiKey";
    public string ClientId { get; set; }
    public KeyLocation KeyLocation { get; set; }

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
    HeaderOrQuery,
}
