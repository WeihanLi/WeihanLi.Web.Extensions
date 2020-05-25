using System;
using Microsoft.AspNetCore.Http;
using WeihanLi.Common.Logging;
using WeihanLi.Web.Extensions;

namespace WeihanLi.Web.Services
{
    public class HttpContextLoggingEnricher : ILogHelperLoggingEnricher
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly Action<LogHelperLoggingEvent, HttpContext> _enrichAction;

        public HttpContextLoggingEnricher(IHttpContextAccessor contextAccessor) : this(contextAccessor, null)
        {
        }

        public HttpContextLoggingEnricher(IHttpContextAccessor contextAccessor, Action<LogHelperLoggingEvent, HttpContext> enrichAction)
        {
            _contextAccessor = contextAccessor;
            if (enrichAction == null)
            {
                _enrichAction = (logEvent, httpContext) =>
                {
                    logEvent.AddProperty("RequestIP", httpContext.GetUserIP());
                    logEvent.AddProperty("RequestPath", httpContext.Request.Path);
                    logEvent.AddProperty("RequestMethod", httpContext.Request.Method);

                    logEvent.AddProperty("TraceId", httpContext.Request.Headers["TraceId"].ToString());
                    logEvent.AddProperty("Referer", httpContext.Request.Headers["Referer"].ToString());
                };
            }
            else
            {
                _enrichAction = enrichAction;
            }
        }

        public void Enrich(LogHelperLoggingEvent loggingEvent)
        {
            if (null != _contextAccessor?.HttpContext)
            {
                _enrichAction.Invoke(loggingEvent, _contextAccessor.HttpContext);
            }
        }
    }
}
