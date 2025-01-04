// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using WeihanLi.Extensions;

namespace WeihanLi.Web.Authorization.Jwt;

internal sealed class JsonWebTokenOptionsSetup : IPostConfigureOptions<JsonWebTokenOptions>
{
    public void PostConfigure(string? name, JsonWebTokenOptions options)
    {
        if (options.SigningCredentialsFactory is null)
        {
            if (options.SecretKey.IsNotNullOrWhiteSpace())
            {
                options.SigningCredentialsFactory = () => new SigningCredentials(new SymmetricSecurityKey(options.SecretKey.GetBytes()), SecurityAlgorithms.HmacSha256);
            }
        }
        ArgumentNullException.ThrowIfNull(options.SigningCredentialsFactory);
        options.SigningCredentials = options.SigningCredentialsFactory.Invoke();
        options.RefreshTokenSigningCredentials = options.RefreshTokenSigningCredentials is null
            ? options.SigningCredentials
            : options.RefreshTokenSigningCredentialsFactory?.Invoke();
    }
}
