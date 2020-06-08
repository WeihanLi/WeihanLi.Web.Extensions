using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authentication.QueryAuthentication;

namespace WeihanLi.Web.Authentication
{
    public static class AuthenticationBuilderExtension
    {
        #region AddHeader

        public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder)
        {
            return builder.AddHeader(HeaderAuthenticationDefaults.AuthenticationSchema);
        }

        public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, string schema)
        {
            return builder.AddHeader(schema, _ => { });
        }

        public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder,
            Action<HeaderAuthenticationOptions> configureOptions)
        {
            return builder.AddHeader(HeaderAuthenticationDefaults.AuthenticationSchema,
                configureOptions);
        }

        public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, string schema,
            Action<HeaderAuthenticationOptions> configureOptions)
        {
            if (null != configureOptions)
            {
                builder.Services.Configure(configureOptions);
            }
            return builder.AddScheme<HeaderAuthenticationOptions, HeaderAuthenticationHandler>(schema,
                configureOptions);
        }

        #endregion AddHeader

        #region AddQuery

        public static AuthenticationBuilder AddQuery(this AuthenticationBuilder builder)
        {
            return builder.AddQuery(QueryAuthenticationDefaults.AuthenticationSchema);
        }

        public static AuthenticationBuilder AddQuery(this AuthenticationBuilder builder, string schema)
        {
            return builder.AddQuery(schema, _ => { });
        }

        public static AuthenticationBuilder AddQuery(this AuthenticationBuilder builder,
            Action<QueryAuthenticationOptions> configureOptions)
        {
            return builder.AddQuery(QueryAuthenticationDefaults.AuthenticationSchema,
                configureOptions);
        }

        public static AuthenticationBuilder AddQuery(this AuthenticationBuilder builder, string schema,
            Action<QueryAuthenticationOptions> configureOptions)
        {
            if (null != configureOptions)
            {
                builder.Services.Configure(configureOptions);
            }
            return builder.AddScheme<QueryAuthenticationOptions, QueryAuthenticationHandler>(schema,
                configureOptions);
        }

        #endregion AddQuery
    }
}
