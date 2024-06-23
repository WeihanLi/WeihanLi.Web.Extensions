// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WeihanLi.Common.Services;
using WeihanLi.Extensions;
using WeihanLi.Web.Authorization.Token;

namespace WeihanLi.Web.Authorization.Jwt;

public class JsonWebTokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly JwtTokenOptions _tokenOptions;

    private readonly Lazy<TokenValidationParameters>
        _lazyTokenValidationParameters,
        _lazyRefreshTokenValidationParameters;

    public JsonWebTokenService(IHttpContextAccessor httpContextAccessor, IOptions<JwtTokenOptions> tokenOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenOptions = tokenOptions.Value;
        _lazyTokenValidationParameters = new(() =>
            _tokenOptions.GetTokenValidationParameters());
        _lazyRefreshTokenValidationParameters = new(() =>
            _tokenOptions.GetTokenValidationParameters(parameters =>
            {
                parameters.ValidAudience = GetRefreshTokenAudience();
                parameters.IssuerSigningKey = _tokenOptions.RefreshTokenSigningCredentials.Key;
            })
        );
    }

    public virtual Task<TokenEntity> GenerateToken(params Claim[] claims)
        => GenerateTokenInternal(_tokenOptions.EnableRefreshToken, claims);

    public virtual Task<TokenValidationResult> ValidateToken(string token)
    {
        return _tokenHandler.ValidateTokenAsync(token, _lazyTokenValidationParameters.Value);
    }

    public virtual async Task<TokenEntity> RefreshToken(string refreshToken)
    {
        var refreshTokenValidateResult = await _tokenHandler.ValidateTokenAsync(refreshToken, _lazyRefreshTokenValidationParameters.Value);
        if (!refreshTokenValidateResult.IsValid || _tokenOptions.RefreshTokenValidator?.Invoke(refreshTokenValidateResult, _httpContextAccessor.HttpContext) == false)
        {
            throw new InvalidOperationException("Invalid RefreshToken", refreshTokenValidateResult.Exception);
        }
        var renewRefreshToken = _tokenOptions.RenewRefreshTokenPredicate?.Invoke(refreshTokenValidateResult);
        return await GenerateTokenInternal(renewRefreshToken.GetValueOrDefault(),
            refreshTokenValidateResult.Claims
                .Where(x => x.Key != JwtRegisteredClaimNames.Jti)
                .Select(c => new Claim(c.Key, c.Value.ToString() ?? string.Empty)).ToArray()
            );
    }

    protected virtual Task<string> GetRefreshToken(Claim[] claims, string jti)
    {
        var claimList = new List<Claim>((claims ?? [])
            .Where(c => c.Type != _tokenOptions.RefreshTokenOwnerClaimType)
            .Union(new[] { new Claim(_tokenOptions.RefreshTokenOwnerClaimType, jti) })
        );

        claimList.RemoveAll(c =>
            JwtInternalClaimTypes.Contains(c.Type)
            || c.Type == JwtRegisteredClaimNames.Jti);
        var jtiNew = _tokenOptions.JtiGenerator?.Invoke() ?? GuidIdGenerator.Instance.NewId();
        claimList.Add(new(JwtRegisteredClaimNames.Jti, jtiNew));
        var now = DateTimeOffset.UtcNow;
        claimList.Add(new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64));
        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: GetRefreshTokenAudience(),
            claims: claimList,
            notBefore: now.UtcDateTime,
            expires: now.Add(_tokenOptions.RefreshTokenValidFor).UtcDateTime,
            signingCredentials: _tokenOptions.RefreshTokenSigningCredentials);
        var encodedJwt = _tokenHandler.WriteToken(jwt);
        return encodedJwt.WrapTask();
    }

    private static readonly HashSet<string> JwtInternalClaimTypes =
    [
        "iss",
        "exp",
        "aud",
        "nbf",
        "iat"
    ];

    private async Task<TokenEntity> GenerateTokenInternal(bool refreshToken, Claim[] claims)
    {
        var now = DateTimeOffset.UtcNow;
        var claimList = new List<Claim>()
        {
            new (JwtRegisteredClaimNames.Iat, now.ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64)
        };
        if (claims != null)
        {
            claimList.AddRange(
                claims.Where(x => !JwtInternalClaimTypes.Contains(x.Type))
            );
        }
        var jti = claimList.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (jti.IsNullOrEmpty())
        {
            jti = _tokenOptions.JtiGenerator?.Invoke() ?? GuidIdGenerator.Instance.NewId();
            claimList.Add(new(JwtRegisteredClaimNames.Jti, jti));
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
            RefreshToken = await GetRefreshToken(claims, jti)
        } : new TokenEntity()
        {
            AccessToken = encodedJwt,
            ExpiresIn = (int)_tokenOptions.ValidFor.TotalSeconds
        };
        return response;
    }

    private string GetRefreshTokenAudience() => $"{_tokenOptions.Audience}_RefreshToken";
}
