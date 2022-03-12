// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Extensions;

public static class HttpContextExtension
{
    /// <summary>
    /// GetUserIP
    /// </summary>
    /// <param name="httpContext">httpContext</param>
    /// <param name="realIPHeader">realIPHeader, default `X-Forwarded-For`</param>
    /// <returns>user ip</returns>
    public static string GetUserIP(this HttpContext httpContext, string realIPHeader = "X-Forwarded-For")
    {
        if (httpContext is null)
        {
            return null;
        }

        return httpContext.Request.Headers.TryGetValue(realIPHeader, out var ip)
            ? ip.ToString()
            : httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }

    /// <summary>
    /// GetUserId from claims, get from `nameid`/ClaimTypes.NameIdentifier by default
    /// </summary>
    /// <param name="principal">principal</param>
    /// <returns></returns>
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var userId = GetUserId(principal, ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        userId = GetUserId(principal, "nameid");
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        return GetUserId(principal, "sub");
    }

    /// <summary>
    /// GetUserId from claims, get from `nameid`/ClaimTypes.NameIdentifier by default
    /// </summary>
    /// <typeparam name="T">userId type</typeparam>
    /// <param name="principal">principal</param>
    /// <returns></returns>
    public static T GetUserId<T>(this ClaimsPrincipal principal)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)principal.GetUserId();

        return GetUserId(principal).ToOrDefault<T>();
    }

    public static T GetUserId<T>(this ClaimsPrincipal principal, string claimType)
    {
        return GetUserId(principal, claimType).ToOrDefault<T>();
    }

    public static string GetUserId(this ClaimsPrincipal principal, string claimType)
    {
        if (principal?.HasClaim(c => c.Type == claimType) == true)
            return principal.FindFirst(claimType).Value;

        return null;
    }
}
