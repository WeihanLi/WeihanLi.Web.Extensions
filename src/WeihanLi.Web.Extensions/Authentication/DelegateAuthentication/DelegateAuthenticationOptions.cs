// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;

namespace WeihanLi.Web.Authentication.DelegateAuthentication;

public sealed class DelegateAuthenticationOptions : AuthenticationSchemeOptions
{
    public Func<HttpContext, Task<bool>> Validator { get; set; } = _ => Task.FromResult(false);
    public Func<HttpContext, Task<IReadOnlyCollection<Claim>>>? ClaimsGenerator { get; set; }
}
