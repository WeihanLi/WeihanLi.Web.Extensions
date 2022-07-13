// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AuthenticateResult = Microsoft.AspNetCore.Authentication.AuthenticateResult;

namespace WeihanLi.Web.Authentication.BasicAuthentication;
public sealed class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader[("Basic".Length + 1)..];
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }
            var array = token.Split(':');
            if (array.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }

            var valid = await Options.UserCredentialValidator.Invoke(Request.HttpContext, array[0], array[1]);
            if (valid)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, array[0]),
                    new Claim("issuer", ClaimsIssuer),
                };
                return AuthenticateResult.Success(new AuthenticationTicket(
                    new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(claims, Scheme.Name)
                    }), Scheme.Name));
            }
            return AuthenticateResult.Fail("Invalid user credential");
        }
        return AuthenticateResult.NoResult();
    }
}
