// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace WeihanLi.Web.Authorization.Token;

public interface ITokenService
{
    Task<TokenEntity> GenerateToken(params Claim[] claims);

    Task<TokenValidationResult> ValidateToken(string token);

    Task<TokenEntity> RefreshToken(string refreshToken);
}
