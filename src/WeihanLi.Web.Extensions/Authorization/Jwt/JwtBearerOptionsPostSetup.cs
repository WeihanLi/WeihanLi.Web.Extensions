﻿// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WeihanLi.Web.Authorization.Jwt;

internal sealed class JwtBearerOptionsPostSetup :
    IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JsonWebTokenOptions> _options;

    public JwtBearerOptionsPostSetup(IOptions<JsonWebTokenOptions> options)
    {
        _options = options;
    }

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        options.Audience = _options.Value.Audience;
        options.ClaimsIssuer = _options.Value.Issuer;
        options.TokenValidationParameters = _options.Value.GetTokenValidationParameters();
    }
}
