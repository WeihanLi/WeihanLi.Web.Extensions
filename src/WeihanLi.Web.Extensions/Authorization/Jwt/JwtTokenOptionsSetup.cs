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
        if (options.SigningCredentialsFactory is null)
        {
            if (options.SecretKey.IsNotNullOrWhiteSpace())
            {
                options.SigningCredentialsFactory = () => new SigningCredentials(new SymmetricSecurityKey(options.SecretKey.GetBytes()), SecurityAlgorithms.HmacSha256);
            }
        }
        Guard.NotNull(options.SigningCredentialsFactory);
        options.SigningCredentials = options.SigningCredentialsFactory();
        options.RefreshTokenSigningCredentials = options.RefreshTokenSigningCredentials is null
            ? options.SigningCredentials
            : options.RefreshTokenSigningCredentialsFactory()
            ;
    }
}
