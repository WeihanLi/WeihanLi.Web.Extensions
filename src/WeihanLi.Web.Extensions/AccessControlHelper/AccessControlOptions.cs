// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.AccessControlHelper;

public sealed class AccessControlOptions
{
    private Func<HttpContext, string?> _accessKeyResolver = context =>
        context.Request.Headers.TryGetValue("X-Access-Key", out var val) ? val.ToString() : null;
    public bool UseAsDefaultPolicy { get; set; }

    public Func<HttpContext, string?> AccessKeyResolver
    {
        get => _accessKeyResolver;
        set => _accessKeyResolver = value ?? throw new ArgumentNullException(nameof(AccessKeyResolver));
    }

    public Func<HttpContext, Task>? DefaultUnauthorizedOperation { get; set; }
}
