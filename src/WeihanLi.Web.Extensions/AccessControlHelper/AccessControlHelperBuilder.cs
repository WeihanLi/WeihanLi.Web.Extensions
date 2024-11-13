// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.AccessControlHelper;

public interface IAccessControlHelperBuilder
{
    IServiceCollection Services { get; }
}

internal sealed class AccessControlHelperBuilder(IServiceCollection services) : IAccessControlHelperBuilder
{
    public IServiceCollection Services { get; } = services;
}
