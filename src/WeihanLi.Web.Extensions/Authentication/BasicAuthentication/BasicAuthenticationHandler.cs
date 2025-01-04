// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WeihanLi.Extensions;
using AuthenticateResult = Microsoft.AspNetCore.Authentication.AuthenticateResult;

namespace WeihanLi.Web.Authentication.BasicAuthentication;
public sealed class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader[("Basic ".Length)..];
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }

            string userName, password;
            try
            {
                var array = Convert.FromBase64String(token).GetString().Split(':');
                if (array.Length != 2)
                {
                    return AuthenticateResult.Fail("Invalid Authorization header");
                }

                userName = array[0];
                password = array[1];
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }


            var valid = await Options.UserCredentialValidator.Invoke(Request.HttpContext, userName, password);
            if (valid)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim("issuer", ClaimsIssuer),
                };
                return AuthenticateResult.Success(new AuthenticationTicket(
                    new ClaimsPrincipal([
                        new ClaimsIdentity(claims, Scheme.Name)
                    ]), Scheme.Name));
            }
            return AuthenticateResult.Fail("Invalid user credential");
        }
        return AuthenticateResult.NoResult();
    }
}
