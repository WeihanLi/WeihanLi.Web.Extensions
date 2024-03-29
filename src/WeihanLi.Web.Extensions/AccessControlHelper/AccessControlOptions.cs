﻿// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.AccessControlHelper;

public sealed class AccessControlOptions
{
    public bool UseAsDefaultPolicy { get; set; }

    public Func<HttpContext, string> AccessKeyResolver { get; set; } = context =>
        context.Request.Headers.TryGetValue("X-Access-Key", out var val) ? val.ToString() : null;

    public Func<HttpContext, Task> DefaultUnauthorizedOperation { get; set; }
}
