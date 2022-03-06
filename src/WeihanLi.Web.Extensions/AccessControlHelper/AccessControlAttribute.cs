// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using WeihanLi.Web.Filters;

namespace WeihanLi.Web.AccessControlHelper;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AccessControlAttribute : AuthorizationFilterAttribute
{
    public string AccessKey { get; set; }

    public override void OnAuthorization(AuthorizationFilterContext filterContext)
    {
        if (filterContext == null)
            throw new ArgumentNullException(nameof(filterContext));

        var isDefinedNoControl = filterContext.ActionDescriptor.IsDefined(typeof(NoAccessControlAttribute), true);

        if (!isDefinedNoControl)
        {
            var accessStrategy = filterContext.HttpContext.RequestServices.GetService<IResourceAccessStrategy>();
            if (accessStrategy is null)
                throw new ArgumentException("IResourceAccessStrategy not initialized，please register your ResourceAccessStrategy", nameof(IResourceAccessStrategy));

            if (!accessStrategy.IsCanAccess(AccessKey))
            {
                //if Ajax request
                filterContext.Result = filterContext.HttpContext.Request.IsAjaxRequest() ?
                    accessStrategy.DisallowedAjaxResult :
                    accessStrategy.DisallowedCommonResult;
            }
        }
    }
}

internal static class AjaxRequestExtensions
{
    public static bool IsAjaxRequest(this Microsoft.AspNetCore.Http.HttpRequest request)
    {
        return request?.Headers != null && string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDefined(this Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor,
        Type attributeType, bool inherit)
    {
        if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            if (controllerActionDescriptor.MethodInfo.GetCustomAttribute(attributeType) == null)
            {
                if (inherit && controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute(attributeType) != null)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }
}
