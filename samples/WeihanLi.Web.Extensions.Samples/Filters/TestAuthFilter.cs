// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using WeihanLi.Web.Filters;

namespace WeihanLi.Web.Extensions.Samples.Filters;

public class TestAuthFilter : AuthorizationFilterAttribute
{
    public required string Role { get; set; }

    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.IsEffectivePolicy(this)) return;
        Console.WriteLine($"{nameof(TestAuthFilter)}({Role}) is executing");
    }
}
