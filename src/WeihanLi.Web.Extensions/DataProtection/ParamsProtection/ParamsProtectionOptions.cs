// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using WeihanLi.Extensions;

namespace WeihanLi.Web.DataProtection.ParamsProtection;

public sealed class ParamsProtectionOptions
{
    private string[] _protectParams = [];

    /// <summary>
    /// ProtectorPurpose
    /// </summary>
    public string? ProtectorPurpose { get; set; } = "ParamsProtection";

    /// <summary>
    /// ExpiresIn, minutes
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Enabled for paramsProtection
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Allow unprotected params
    /// </summary>
    public bool AllowUnprotectedParams { get; set; }

    /// <summary>
    /// Invalid request response http status code
    /// refer to https://restfulapi.net/http-status-codes/
    /// </summary>
    public int InvalidRequestStatusCode { get; set; } = 412;

    /// <summary>
    /// the params to protect
    /// </summary>
    public string[] ProtectParams
    {
        get => _protectParams;
        set
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (value is not null)
            {
                _protectParams = value;
            }
        }
    }

    /// <summary>
    /// whether the NeedProtectFunc is enabled
    /// </summary>
    public bool ParamValueProtectFuncEnabled { get; set; }

    /// <summary>
    /// whether the parameter should be protected
    /// </summary>
    public Func<string?, bool> ParamValueNeedProtectFunc { get; set; } = str => long.TryParse(str, out _);

    /// <summary>
    /// whether the response should be protected
    /// </summary>
    internal IDictionary<Type, string> NeedProtectResponseValues { get; } = new Dictionary<Type, string>()
        {
            { typeof(ObjectResult), "Value"}
        };

    /// <summary>
    /// Add type and value ToProtectValues
    /// </summary>
    /// <typeparam name="TResult">TResult</typeparam>
    /// <param name="valueExpression">the value of the type to protect</param>
    public void AddProtectValue<TResult>(Expression<Func<TResult, object>> valueExpression) where TResult : class, IActionResult
    {
        NeedProtectResponseValues[typeof(TResult)] = valueExpression.GetMemberInfo().Name;
    }
}
