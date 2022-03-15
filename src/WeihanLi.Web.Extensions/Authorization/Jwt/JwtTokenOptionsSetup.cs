// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Authorization.Jwt;

internal sealed class JwtTokenOptionsSetup : IPostConfigureOptions<JwtTokenOptions>
{
    public void PostConfigure(string name, JwtTokenOptions options)
    {
        if (options.SecurityKeyFactory is null)
        {
            if (options.SecretKey.IsNotNullOrWhiteSpace())
            {
                options.SecurityKeyFactory = () => new SymmetricSecurityKey(options.SecretKey.GetBytes());
            }
        }
        Guard.NotNull(options.SecurityKeyFactory);
        options.SecurityKey = options.SecurityKeyFactory();
        options.SigningCredentials = new SigningCredentials(options.SecurityKey, options.SecurityAlgorithm);
    }
}
