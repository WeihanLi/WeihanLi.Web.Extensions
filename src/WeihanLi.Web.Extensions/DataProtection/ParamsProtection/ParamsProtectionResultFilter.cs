// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace WeihanLi.Web.DataProtection.ParamsProtection;

public sealed class ParamsProtectionResultFilter : IResultFilter
{
    private readonly IDataProtector _protector;
    private readonly ParamsProtectionOptions _option;
    private readonly ILogger _logger;

    public ParamsProtectionResultFilter(IDataProtectionProvider protectionProvider, IOptions<ParamsProtectionOptions> options, ILogger<ParamsProtectionResultFilter> logger)
    {
        _logger = logger;
        _option = options.Value;

        _protector = protectionProvider.CreateProtector(_option.ProtectorPurpose ?? ParamsProtectionHelper.DefaultPurpose);

        if (_option.ExpiresIn.GetValueOrDefault(0) > 0)
        {
            _protector = _protector.ToTimeLimitedDataProtector();
        }
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (_option.Enabled && _option.ProtectParams.Length > 0)
        {
            foreach (var pair in _option.NeedProtectResponseValues)
            {
                if (pair.Key.IsInstanceOfType(context.Result))
                {
                    var prop = CacheUtil.GetTypeProperties(pair.Key).FirstOrDefault(p => p.Name == pair.Value);
                    var val = prop?.GetValueGetter()?.Invoke(context.Result);
                    if (val != null)
                    {
                        _logger.LogDebug($"ParamsProtector is protecting {pair.Key.FullName} Value");

                        var obj = JToken.FromObject(val);
                        ParamsProtectionHelper.ProtectParams(obj, _protector, _option);
                        prop.GetValueSetter()?.Invoke(context.Result, obj);
                    }
                }
            }
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
