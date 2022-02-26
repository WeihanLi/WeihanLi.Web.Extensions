using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WeihanLi.Web.AccessControlHelper;

/// <summary>
/// NoAccessControl
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
    }
}
