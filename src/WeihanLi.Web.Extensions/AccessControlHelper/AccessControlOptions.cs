// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;

namespace WeihanLi.Web.AccessControlHelper;

public class AccessControlOptions
{
    public bool UseAsDefaultPolicy { get; set; }

    public Func<HttpContext, string> AccessKeyResolver { get; set; } = context =>
        context.Request.Headers.TryGetValue("X-Access-Key", out var val) ? val.ToString() : null;

    public Func<HttpContext, Task> DefaultUnauthorizedOperation { get; set; }
}
