using Microsoft.AspNetCore.Builder;

namespace WeihanLi.Web.Extensions
{
    public static class MiddlewareExtension
    {
        /// <summary>
        /// UseCustomExceptionHandler
        /// </summary>
        /// <param name="applicationBuilder">applicationBuilder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<Middleware.CustomExceptionHandlerMiddleware>();
            return applicationBuilder;
        }
    }
}
