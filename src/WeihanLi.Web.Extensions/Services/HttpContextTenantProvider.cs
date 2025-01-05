// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Services;

public sealed class HttpContextTenantProviderOptions
{
    private Func<HttpContext, string?> _tenantIdFactory = context => context.User.FindFirst("tenantId")?.Value;

    private Func<HttpContext, TenantInfo?> _tenantInfoFactory = context =>
          {
              var tenantId = context.User.FindFirst("tenantId")?.Value;
              if (string.IsNullOrEmpty(tenantId))
              {
                  return null;
              }
              return new TenantInfo()
              {
                  TenantId = tenantId,
                  TenantName = context.User.FindFirst("tenantName")?.Value
              };
          };

    public Func<HttpContext, string?> TenantIdFactory
    {
        get => _tenantIdFactory;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(TenantIdFactory));
            _tenantIdFactory = value;
        }
    }

    public Func<HttpContext, TenantInfo?> TenantInfoFactory
    {
        get => _tenantInfoFactory;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(TenantInfoFactory));
            _tenantInfoFactory = value;
        }
    }
}

public sealed class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<HttpContextTenantProviderOptions> _options;

    public HttpContextTenantProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<HttpContextTenantProviderOptions> options
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public string? GetTenantId()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            return null;
        }

        return _options.Value.TenantIdFactory.Invoke(_httpContextAccessor.HttpContext);
    }

    public TenantInfo? GetTenantInfo()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            return null;
        }

        return _options.Value.TenantInfoFactory.Invoke(_httpContextAccessor.HttpContext);
    }
}
