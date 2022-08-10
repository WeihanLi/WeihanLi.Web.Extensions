// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common;
using WeihanLi.Web.DataProtection.ParamsProtection;

namespace WeihanLi.Web.DataProtection;

public static class DataProtectionBuilderExtensions
{
    /// <summary>
    /// AddParamsProtection
    /// </summary>
    /// <param name="builder">dataProtectionBuilder</param>
    /// <returns></returns>
    public static IDataProtectionBuilder AddParamsProtection(this IDataProtectionBuilder builder)
    {
        Guard.NotNull(builder);

        builder.Services.Configure<MvcOptions>(action =>
            {
                action.Filters.Add<ParamsProtectionResourceFilter>();
                action.Filters.Add<ParamsProtectionResultFilter>();
            });

        return builder;
    }

    /// <summary>
    /// AddParamsProtection
    /// </summary>
    /// <param name="builder">dataProtectionBuilder</param>
    /// <param name="optionsAction">options config action</param>
    /// <returns></returns>
    public static IDataProtectionBuilder AddParamsProtection(this IDataProtectionBuilder builder, Action<ParamsProtectionOptions> optionsAction)
    {
        if (null == builder)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (null == optionsAction)
        {
            throw new ArgumentNullException(nameof(optionsAction));
        }

        builder.Services.Configure(optionsAction);
        builder.AddParamsProtection();
        return builder;
    }
}
