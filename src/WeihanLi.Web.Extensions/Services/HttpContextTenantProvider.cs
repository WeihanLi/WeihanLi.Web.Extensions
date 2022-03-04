// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Services;

public sealed class HttpContextTenantProviderOptions
{
    private Func<HttpContext, string> _tenantIdFactory = context => context?.User?.FindFirst("tenantId")?.Value;

    private Func<HttpContext, TenantInfo> _tenantInfoFactory = context =>
          {
              var tenantId = context?.User?.FindFirst("tenantId")?.Value;
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

    public Func<HttpContext, string> TenantIdFactory
    {
        get => _tenantIdFactory;
        set
        {
            if (value != null)
            {
                _tenantIdFactory = value;
            }
        }
    }

    public Func<HttpContext, TenantInfo> TenantInfoFactory
    {
        get => _tenantInfoFactory;
        set
        {
            if (value != null)
            {
                _tenantInfoFactory = value;
            }
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

    public string GetTenantId()
    {
        return _options.Value.TenantIdFactory.Invoke(_httpContextAccessor.HttpContext);
    }

    public TenantInfo GetTenantInfo()
    {
        return _options.Value.TenantInfoFactory.Invoke(_httpContextAccessor.HttpContext);
    }
}
