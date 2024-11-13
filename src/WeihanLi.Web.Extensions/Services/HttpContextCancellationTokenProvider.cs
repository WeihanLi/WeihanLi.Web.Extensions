// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Services;

namespace WeihanLi.Web.Services;

public class HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor) : ICancellationTokenProvider
{
    public virtual CancellationToken GetCancellationToken()
    {
        return httpContextAccessor.HttpContext?.RequestAborted ?? default;
    }
}
