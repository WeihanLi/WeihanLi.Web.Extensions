// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace WeihanLi.Web.AccessControlHelper;

public interface IAccessControlHelperBuilder
{
    IServiceCollection Services { get; }
}

internal sealed class AccessControlHelperBuilder : IAccessControlHelperBuilder
{
    public IServiceCollection Services { get; }

    public AccessControlHelperBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
