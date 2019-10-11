using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WeihanLi.Web.Middlewares
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
}
