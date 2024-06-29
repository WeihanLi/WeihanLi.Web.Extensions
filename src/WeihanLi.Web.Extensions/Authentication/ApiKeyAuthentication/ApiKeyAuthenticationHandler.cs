// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
#if NET8_0_OR_GREATER
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }
    
    [Obsolete("ISystemClock is obsolete, use TimeProvider on AuthenticationSchemeOptions instead.")]
#endif
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        StringValues keyValues;
        var keyExists = Options.KeyLocation switch
        {
            KeyLocation.Query => Request.Query.TryGetValue(Options.ApiKeyName, out keyValues),
            KeyLocation.HeaderOrQuery => Request.Headers.TryGetValue(Options.ApiKeyName, out keyValues) || Request.Query.TryGetValue(Options.ApiKeyName, out keyValues),
            KeyLocation.QueryOrHeader => Request.Query.TryGetValue(Options.ApiKeyName, out keyValues) || Request.Headers.TryGetValue(Options.ApiKeyName, out keyValues),
            _ => Request.Headers.TryGetValue(Options.ApiKeyName, out keyValues),
        };
        if (!keyExists)
            return AuthenticateResult.NoResult();

        var validator = Options.ApiKeyValidator ?? ((_, keyValue) => Task.FromResult(string.Equals(Options.ApiKey, keyValue)));
        if (await validator.Invoke(Context, keyValues.ToString()))
        {
            var claims = new[]
            {
                new Claim("issuer", ClaimsIssuer),
            }.Union(Options.ClaimsGenerator?.Invoke(Context, Options) ?? []);
            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(claims, Scheme.Name)
                    }), Scheme.Name)
            );
        }
        return AuthenticateResult.Fail("Invalid ApiKey");
    }
}
