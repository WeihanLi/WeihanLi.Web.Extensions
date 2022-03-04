// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WeihanLi.Web.Authentication.HeaderAuthentication;

public sealed class HeaderAuthenticationHandler : AuthenticationHandler<HeaderAuthenticationOptions>
{
    public HeaderAuthenticationHandler(IOptionsMonitor<HeaderAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (await Options.AuthenticationValidator(Context))
        {
            var claims = new List<Claim>();
            if (Request.Headers.TryGetValue(Options.UserIdHeaderName, out var userIdValues))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdValues.ToString()));
            }
            if (Request.Headers.TryGetValue(Options.UserNameHeaderName, out var userNameValues))
            {
                claims.Add(new Claim(ClaimTypes.Name, userNameValues.ToString()));
            }
            if (Request.Headers.TryGetValue(Options.UserRolesHeaderName, out var userRolesValues))
            {
                var userRoles = userRolesValues.ToString()
                    .Split(new[] { Options.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            if (Options.AdditionalHeaderToClaims.Count > 0)
            {
                foreach (var headerToClaim in Options.AdditionalHeaderToClaims)
                {
                    if (Request.Headers.TryGetValue(headerToClaim.Key, out var headerValues))
                    {
                        foreach (var val in headerValues.ToString().Split(new[] { Options.Delimiter }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            claims.Add(new Claim(headerToClaim.Value, val));
                        }
                    }
                }
            }

            // claims identity 's authentication type can not be null https://stackoverflow.com/questions/45261732/user-identity-isauthenticated-always-false-in-net-core-custom-authentication
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            var ticket = new AuthenticationTicket(
                principal,
                Scheme.Name
            );
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.NoResult();
    }
}
