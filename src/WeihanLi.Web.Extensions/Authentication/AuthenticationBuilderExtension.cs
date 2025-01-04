// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.BasicAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authentication.QueryAuthentication;

namespace WeihanLi.Web.Authentication;

public static class AuthenticationBuilderExtension
{
    #region AddApiKey

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
    {
        return builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationSchema);
    }

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string schema)
    {
        return builder.AddApiKey(schema, _ => { });
    }

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder,
        Action<ApiKeyAuthenticationOptions> configureOptions)
    {
        return builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationSchema,
            configureOptions);
    }

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string schema,
        Action<ApiKeyAuthenticationOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        builder.Services.Configure(configureOptions);
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(schema,
            configureOptions);
    }

    #endregion AddApiKey

    #region AddBasic

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
    {
        return builder.AddBasic(BasicAuthenticationDefaults.AuthenticationSchema);
    }

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string schema)
    {
        return builder.AddBasic(schema, _ => { });
    }

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder,
        Action<BasicAuthenticationOptions> configureOptions)
    {
        return builder.AddBasic(BasicAuthenticationDefaults.AuthenticationSchema,
            configureOptions);
    }

    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string schema,
        Action<BasicAuthenticationOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        builder.Services.Configure(configureOptions);
        return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(schema,
            configureOptions);
    }

    #endregion AddApiKey

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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        builder.Services.Configure(configureOptions);
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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        builder.Services.Configure(configureOptions);
        return builder.AddScheme<QueryAuthenticationOptions, QueryAuthenticationHandler>(schema,
            configureOptions);
    }

    #endregion AddQuery
}
