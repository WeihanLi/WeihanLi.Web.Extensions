using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WeihanLi.Web.Middleware
{
    public class CustomExceptionHandlerOptions
    {
        public Func<HttpContext, ILogger, Exception, Task> OnException { get; set; } =
            (context, logger, exception) =>
            {
                logger.LogError(exception, $"Request exception, requestId: {context.TraceIdentifier}");
                return Task.CompletedTask;
            };

        public Func<HttpContext, ILogger, Task> OnRequestAborted { get; set; } =
            (context, logger) =>
            {
                logger.LogInformation($"Request aborted, requestId: {context.TraceIdentifier}");
                return Task.CompletedTask;
            };
    }

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
            catch (Exception ex)
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
