// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Authorization.Jwt;

public sealed class JsonWebTokenOptions
{
    /// <summary>
    /// "iss" (Issuer) Claim
    /// </summary>
    /// <remarks>The "iss" (issuer) claim identifies the principal that issued the
    ///   JWT.  The processing of this claim is generally application specific.
    ///   The "iss" value is a case-sensitive string containing a StringOrURI
    ///   value.  Use of this claim is OPTIONAL.</remarks>
    public string? Issuer { get; set; }

    /// <summary>
    /// "aud" (Audience) Claim
    /// </summary>
    /// <remarks>The "aud" (audience) claim identifies the recipients that the JWT is
    ///   intended for.  Each principal intended to process the JWT MUST
    ///   identify itself with a value in the audience claim.  If the principal
    ///   processing the claim does not identify itself with a value in the
    ///   "aud" claim when this claim is present, then the JWT MUST be
    ///   rejected.  In the general case, the "aud" value is an array of case-sensitive strings, each containing a StringOrURI value.
    ///   In the special case when the JWT has one audience, the "aud" value MAY be a
    ///   single case-sensitive string containing a StringOrURI value.  The
    ///   interpretation of audience values is generally application specific.
    ///   Use of this claim is OPTIONAL.</remarks>
    public string? Audience { get; set; }

    /// <summary>
    /// SecretKey used for generate and validate token
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Set the timespan the token will be valid for (default is 1 hour/3600 seconds)
    /// </summary>
    public TimeSpan ValidFor { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the clock skew to apply when validating a time.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// "jti" (JWT ID) Claim (default ID is a GUID)
    /// </summary>
    /// <remarks>
    ///   The "jti" (JWT ID) claim provides a unique identifier for the JWT.
    ///   The identifier value MUST be assigned in a manner that ensures that
    ///   there is a negligible probability that the same value will be
    ///   accidentally assigned to a different data object; if the application
    ///   uses multiple issuers, collisions MUST be prevented among values
    ///   produced by different issuers as well.  The "jti" claim can be used
    ///   to prevent the JWT from being replayed.
    /// </remarks>
    public Func<string>? JtiGenerator { get; set; } = () => GuidIdGenerator.Instance.NewId();

    public Func<SigningCredentials>? SigningCredentialsFactory { get; set; }

    public bool EnableRefreshToken { get; set; }

    public Func<TokenValidationResult, bool>? RenewRefreshTokenPredicate { get; set; }

    public TimeSpan RefreshTokenValidFor { get; set; } = TimeSpan.FromHours(8);

    public Func<SigningCredentials>? RefreshTokenSigningCredentialsFactory { get; set; }

    public string? NameClaimType { get; set; }
    public string? RoleClaimType { get; set; }

    public string RefreshTokenOwnerClaimType { get; set; } = "x-rt-owner";

    public Func<TokenValidationResult, HttpContext, bool>? RefreshTokenValidator { get; set; }

    public Action<TokenValidationParameters>? TokenValidationConfigure { get; set; }

    internal SigningCredentials? SigningCredentials { get; set; }
    internal SigningCredentials? RefreshTokenSigningCredentials { get; set; }

    internal TokenValidationParameters GetTokenValidationParameters(Action<TokenValidationParameters>? parametersAction = null)
    {
        var parameters = new TokenValidationParameters
        {
            // The signing key must match!
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SigningCredentials?.Key,
            // Validate the JWT Issuer (iss) claim
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            // Validate the JWT Audience (aud) claim
            ValidateAudience = true,
            ValidAudience = Audience,
            // Validate the token expiry
            ValidateLifetime = true,
            // If you want to allow a certain amount of clock drift, set that here:
            ClockSkew = ClockSkew
        };

        if (!string.IsNullOrEmpty(NameClaimType))
        {
            parameters.NameClaimType = NameClaimType;
        }
        if (!string.IsNullOrEmpty(RoleClaimType))
        {
            parameters.RoleClaimType = RoleClaimType;
        }

        parametersAction?.Invoke(parameters);
        TokenValidationConfigure?.Invoke(parameters);
        return parameters;
    }
}
