// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Authentication.BasicAuthentication;
public sealed class BasicAuthenticationOptions : AuthenticationSchemeOptions
{
    public string? UserName { get; set; }
    public string? Password { get; set; }

    public Func<HttpContext, string, string, Task<bool>> UserCredentialValidator { get; set; }
        = (context, user, pass) =>
        {
            var options = context.RequestServices.GetRequiredService<IOptions<BasicAuthenticationOptions>>()
                .Value;
            return Task.FromResult(user == options.UserName && pass == options.Password);
        };
}
