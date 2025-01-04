// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WeihanLi.Web.Authentication.DelegateAuthentication;

public sealed class DelegateAuthenticationHandler(
    IOptionsMonitor<DelegateAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<DelegateAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authenticated = await Options.Validator.Invoke(Context);
        if (!authenticated)
            return AuthenticateResult.Fail($"Delegate authentication({Scheme}) failed.");

        List<Claim> claims =
        [
            new("issuer", ClaimsIssuer)
        ];

        if (Options.ClaimsGenerator != null)
        {
            var generatedClaims = await Options.ClaimsGenerator.Invoke(Context);
            if (generatedClaims is { Count: > 0 })
            {
                claims = [.. generatedClaims, .. claims];
            }
        }

        return AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal([
                    new ClaimsIdentity(claims, Scheme.Name)
                ]), Scheme.Name)
        );
    }
}
