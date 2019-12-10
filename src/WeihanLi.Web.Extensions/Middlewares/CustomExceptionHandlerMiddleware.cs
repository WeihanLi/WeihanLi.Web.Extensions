using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WeihanLi.Web.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CustomExceptionHandlerOptions _options;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, IOptions<CustomExceptionHandlerOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (System.Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<CustomExceptionHandlerMiddleware>();
                if (context.RequestAborted.IsCancellationRequested && (ex is TaskCanceledException || ex is OperationCanceledException))
                {
                    _options.OnRequestAborted?.Invoke(context, logger);
                }
                else
                {
                    _options.OnException?.Invoke(context, logger, ex);
                }
            }
        }
    }
}
