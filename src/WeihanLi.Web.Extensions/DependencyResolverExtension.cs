using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using WeihanLi.Common;

namespace WeihanLi.Web.Extensions
{
    public static class DependencyResolverExtensions
    {
        public static IApplicationBuilder UseDependencyResolver(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.Use(async (context, next) =>
            {
                DependencyResolver.SetDependencyResolver(context.RequestServices);
                await next();
            });
            return applicationBuilder;
        }
    }
}
