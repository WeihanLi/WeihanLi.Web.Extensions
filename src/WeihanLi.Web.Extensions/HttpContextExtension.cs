using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace WeihanLi.Web.Extensions
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// GetUserIP
        /// </summary>
        /// <param name="httpContext">httpContext</param>
        /// <param name="realIPHeader">realIPHeader, default `X-Real-IP`</param>
        /// <returns></returns>
        public static string GetUserIP(this HttpContext httpContext, string realIPHeader = "X-Real-IP")
        {
            return httpContext.Request.Headers.TryGetValue(realIPHeader, out var ip) ? ip.ToString() : httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
