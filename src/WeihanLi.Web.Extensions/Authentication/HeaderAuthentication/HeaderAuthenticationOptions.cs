// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Authentication.HeaderAuthentication;

public sealed class HeaderAuthenticationOptions : AuthenticationSchemeOptions
{
    private string _userRolesHeaderName = "UserRoles";
    private string _userNameHeaderName = "UserName";
    private string _userIdHeaderName = "UserId";
    private string _delimiter = ",";

    public string UserIdHeaderName
    {
        get => _userIdHeaderName;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _userIdHeaderName = value;
            }
        }
    }

    public string UserNameHeaderName
    {
        get => _userNameHeaderName;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _userNameHeaderName = value;
            }
        }
    }

    public string UserRolesHeaderName
    {
        get => _userRolesHeaderName;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _userRolesHeaderName = value;
            }
        }
    }

    /// <summary>
    /// AdditionalHeaderToClaims
    /// key: headerName
    /// value: claimType
    /// </summary>
    public Dictionary<string, string> AdditionalHeaderToClaims { get; } = new Dictionary<string, string>();

    private Func<HttpContext, Task<bool>> _authenticationValidator = context =>
    {
        var userIdHeader = context.RequestServices.GetRequiredService<IOptions<HeaderAuthenticationOptions>>().Value.UserIdHeaderName;
        return Task.FromResult(context.Request.Headers.ContainsKey(userIdHeader));
    };

    public Func<HttpContext, Task<bool>> AuthenticationValidator
    {
        get => _authenticationValidator;
        set => _authenticationValidator = value ?? throw new ArgumentNullException(nameof(AuthenticationValidator));
    }

    public string Delimiter
    {
        get => _delimiter;
        set => _delimiter = string.IsNullOrEmpty(value) ? "," : value;
    }
}
