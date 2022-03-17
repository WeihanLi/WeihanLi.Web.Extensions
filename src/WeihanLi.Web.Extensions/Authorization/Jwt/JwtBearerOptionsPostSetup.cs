using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WeihanLi.Web.Authorization.Jwt;

public class JwtBearerOptionsPostSetup:
    IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtTokenOptions> _options;

    public JwtBearerOptionsPostSetup(IOptions<JwtTokenOptions> options)
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