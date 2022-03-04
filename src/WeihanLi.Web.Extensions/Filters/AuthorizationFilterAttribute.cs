// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;
using WeihanLi.Common;

namespace WeihanLi.Web.Filters;

public abstract class AuthorizationFilterAttribute : Attribute, IAuthorizationFilter, IAsyncAuthorizationFilter
{
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
    }

    public virtual Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        Guard.NotNull(context);
        OnAuthorization(context);
        return Task.CompletedTask;
    }
}
