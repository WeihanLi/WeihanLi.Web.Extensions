﻿// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WeihanLi.Common;
using WeihanLi.Extensions;
using WeihanLi.Web.Authorization.Token;

namespace WeihanLi.Web.Authorization.Jwt;

public class JwtTokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly JwtTokenOptions _tokenOptions;
    private readonly Lazy<TokenValidationParameters> _lazyTokenValidationParameters;

    public JwtTokenService(IOptions<JwtTokenOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions.Value;
        _lazyTokenValidationParameters = new(() => _tokenOptions.GetTokenValidationParameters());
    }

    public Task<TokenEntity> GenerateToken(params Claim[] claims)
        => GenerateTokenInternal(_tokenOptions.EnableRefreshToken, claims);

    public Task<TokenValidationResult> ValidateToken(string token)
    {
        return _tokenHandler.ValidateTokenAsync(token, _lazyTokenValidationParameters.Value);
    }

    public virtual async Task<TokenEntity> RefreshToken(string refreshToken)
    {
        // TODO: cache validation parameters
        var validationParameters = _tokenOptions.GetTokenValidationParameters(parameters =>
        {
            parameters.ValidAudience = GetRefreshTokenAudience();
        });

        var refreshTokenValidateResult = await _tokenHandler.ValidateTokenAsync(refreshToken, validationParameters);
        if (!refreshTokenValidateResult.IsValid)
        {
            throw new InvalidOperationException("Invalid RefreshToken", refreshTokenValidateResult.Exception);
        }
        return await GenerateTokenInternal(false, refreshTokenValidateResult.Claims.Select(c => new Claim(c.Key, c.Value.ToString() ?? string.Empty)).ToArray());
    }

    protected virtual Task<string> GetRefreshToken(Claim[] claims)
    {
        var now = DateTimeOffset.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: GetRefreshTokenAudience(),
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.Add(_tokenOptions.ValidFor).UtcDateTime,
            signingCredentials: _tokenOptions.SigningCredentials);
        var encodedJwt = _tokenHandler.WriteToken(jwt);
        return encodedJwt.WrapTask();
    }

    private async Task<TokenEntity> GenerateTokenInternal(bool refreshToken, Claim[] claims)
    {
        var now = DateTimeOffset.UtcNow;
        var claimList = new List<Claim>()
        {
            new (JwtRegisteredClaimNames.Jti, _tokenOptions.JtiGenerator?.Invoke() ?? GuidIdGenerator.Instance.NewId()),
            new (JwtRegisteredClaimNames.Iat, now.ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64)
        };
        if (claims != null)
        {
            claimList.AddRange(claims);
        }
        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claimList,
            notBefore: now.UtcDateTime,
            expires: now.Add(_tokenOptions.ValidFor).UtcDateTime,
            signingCredentials: _tokenOptions.SigningCredentials);
        var encodedJwt = _tokenHandler.WriteToken(jwt);

        var response = refreshToken ? new TokenEntityWithRefreshToken()
        {
            AccessToken = encodedJwt,
            ExpiresIn = (int)_tokenOptions.ValidFor.TotalSeconds,
            RefreshToken = await GetRefreshToken(claims)
        } : new TokenEntity()
        {
            AccessToken = encodedJwt,
            ExpiresIn = (int)_tokenOptions.ValidFor.TotalSeconds
        };
        return response;
    }

    private string GetRefreshTokenAudience() => $"{_tokenOptions.Audience}_RefreshToken";
}
