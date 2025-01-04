// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using WeihanLi.Common;

namespace WeihanLi.Web.Filters;

/// <summary>
/// Environment filter with allowed environment name
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class EnvironmentFilter : ConditionalFilter
{
    public EnvironmentFilter(params string[] environmentNames)
    {
        Guard.NotNull(environmentNames);
        var allowedEnvironments = environmentNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        ConditionFunc = c =>
        {
            var env = c.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName;
            return allowedEnvironments.Contains(env);
        };
    }
}

/// <summary>
/// Should work only for non-production
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class NonProductionEnvironmentFilter : ConditionalFilter
{
    public NonProductionEnvironmentFilter()
    {
        ConditionFunc = c => c.RequestServices.GetRequiredService<IWebHostEnvironment>()
            .IsProduction() == false;
    }
}
