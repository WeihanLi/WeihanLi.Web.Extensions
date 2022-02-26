// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Authentication.ApiKeyAuthentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authResult = HandleAuthenticateInternal();
        return Task.FromResult(authResult);
    }

    private AuthenticateResult HandleAuthenticateInternal()
    {
        StringValues keyValues;
        var keyExists = Options.KeyLocation switch
        {
            KeyLocation.Query => Request.Query.TryGetValue(Options.ApiKeyName, out keyValues),
            KeyLocation.HeaderOrQuery => Request.Headers.TryGetValue(Options.ApiKeyName, out keyValues) || Request.Query.TryGetValue(Options.ApiKeyName, out keyValues),
            _ => Request.Headers.TryGetValue(Options.ApiKeyName, out keyValues),
        };
        if (!keyExists)
            return AuthenticateResult.NoResult();

        if (keyValues.ToString().Equals(Options.ApiKey))
        {
            var clientId = Options.ClientId.GetValueOrDefault(ApplicationHelper.ApplicationName);
            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(new[]
                    {
                            new ClaimsIdentity(new[]
                            {
                                new Claim(nameof(Options.ClientId), clientId, ClaimValueTypes.String, ClaimsIssuer),
                            }, Scheme.Name)
                    }), Scheme.Name)
            );
        }
        return AuthenticateResult.Fail("Invalid Api-Key");
    }
}
