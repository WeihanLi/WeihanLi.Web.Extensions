// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Text;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace WeihanLi.Web.DataProtection.ParamsProtection;

public class ParamsProtectionResourceFilter : IResourceFilter
{
    private static readonly Lazy<XmlDataSerializer> XmlDataSerializer = new Lazy<XmlDataSerializer>(() => new XmlDataSerializer());
    private readonly IDataProtector _protector;
    private readonly ParamsProtectionOptions _option;

    private readonly ILogger _logger;

    public ParamsProtectionResourceFilter(IDataProtectionProvider protectionProvider, ILogger<ParamsProtectionResourceFilter> logger, IOptions<ParamsProtectionOptions> options)
    {
        _option = options.Value;
        _protector = protectionProvider.CreateProtector(_option.ProtectorPurpose ?? ParamsProtectionHelper.DefaultPurpose);
        if (_option.ExpiresIn.GetValueOrDefault(0) > 0)
        {
            _protector = _protector.ToTimeLimitedDataProtector();
        }

        _logger = logger;
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (_option.Enabled && _option.ProtectParams.Length > 0)
        {
            var request = context.HttpContext.Request;

            // QueryString
            if (request.Query != null && request.Query.Count > 0)
            {
                var queryDic = request.Query.ToDictionary(query => query.Key, query => query.Value);
                foreach (var param in _option.ProtectParams)
                {
                    if (queryDic.ContainsKey(param))
                    {
                        var vals = new List<string>();
                        for (var i = 0; i < queryDic[param].Count; i++)
                        {
                            if (_protector.TryGetUnprotectedValue(_option, queryDic[param][i], out var val))
                            {
                                vals.Add(val);
                            }
                            else
                            {
                                _logger.LogWarning($"Error in unprotect query value: param:{param}");
                                context.Result = new StatusCodeResult(_option.InvalidRequestStatusCode);

                                return;
                            }
                        }
                        queryDic[param] = new StringValues(vals.ToArray());
                    }
                    context.HttpContext.Request.Query = new QueryCollection(queryDic);
                }
            }
            // route value
            if (context.RouteData?.Values != null)
            {
                foreach (var param in _option.ProtectParams)
                {
                    if (context.RouteData.Values.ContainsKey(param))
                    {
                        if (_protector.TryGetUnprotectedValue(_option, context.RouteData.Values[param].ToString(), out var val))
                        {
                            context.RouteData.Values[param] = val;
                        }
                        else
                        {
                            _logger.LogWarning($"Error in un-protect routeValue:{param}");

                            context.Result = new StatusCodeResult(_option.InvalidRequestStatusCode);

                            return;
                        }
                    }
                }
            }

            if (request.Method.EqualsIgnoreCase("POST") || request.Method.EqualsIgnoreCase("PUT"))
            {
                if (request.ContentType.Contains("json"))
                {
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                    {
                        var content = reader.ReadToEnd();
                        var obj = content.JsonToObject<JToken>();
                        try
                        {
                            ParamsProtectionHelper.UnProtectParams(obj, _protector, _option);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Error in unprotect request body");

                            context.Result = new StatusCodeResult(_option.InvalidRequestStatusCode);

                            return;
                        }

                        context.HttpContext.Request.Body = obj.ToJson().GetBytes().ToMemoryStream();
                    }
                } // json body
                else if (request.ContentType.Contains("xml"))
                {
                    // TODO: need test
                    var obj = XmlDataSerializer.Value.Deserialize<JToken>(request.Body.ToByteArray());
                    try
                    {
                        ParamsProtectionHelper.UnProtectParams(obj, _protector, _option);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Error in unprotect request body");

                        context.Result = new StatusCodeResult(_option.InvalidRequestStatusCode);

                        return;
                    }
                    context.HttpContext.Request.Body = XmlDataSerializer.Value.Serialize(obj).ToMemoryStream();
                } // xml body

                // form data
                if (request.HasFormContentType && request.Form != null && request.Form.Count > 0)
                {
                    var formDic = request.Form.ToDictionary(_ => _.Key, _ => _.Value);
                    foreach (var param in _option.ProtectParams)
                    {
                        if (formDic.TryGetValue(param, out var values))
                        {
                            var vals = new List<string>();
                            for (var i = 0; i < values.Count; i++)
                            {
                                if (_protector.TryGetUnprotectedValue(_option, values[i], out var val))
                                {
                                    vals.Add(val);
                                }
                                else
                                {
                                    _logger.LogWarning($"Error in unprotect form data: param:{param}");
                                    context.Result = new StatusCodeResult(_option.InvalidRequestStatusCode);

                                    return;
                                }
                            }
                            formDic[param] = new StringValues(vals.ToArray());
                        }
                    }
                    request.Form = new FormCollection(formDic);
                }
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
