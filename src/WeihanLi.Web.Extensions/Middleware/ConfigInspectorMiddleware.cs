// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Middleware;

public sealed class ConfigInspectorOptions
{
    public string Path { get; set; } = "/config-inspector";
    public bool IncludeEmptyProviders { get; set; }
    public Func<HttpContext, ConfigModel[], Task> ConfigRenderer { get; set; }
}

public sealed class ConfigModel
{
    public string Provider { get; set; }
    public ConfigItemModel[] Items { get; set; }
}

public sealed class ConfigItemModel
{
    public string Key { get; set; }
    public string Value { get; set; }
    public bool Active { get; set; }
}

internal sealed class ConfigInspectorMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext httpContext, IOptions<ConfigInspectorOptions> inspectorOptions)
    {
        if (httpContext.Request.Path.ToString() != inspectorOptions.Value.Path)
        {
            return next(httpContext);
        }

        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            throw new NotSupportedException(
                "Support ConfigurationRoot configuration only, please use the default configuration or implement IConfigurationRoot");
        }

        var inspectorOptionsValue = inspectorOptions.Value;
        var configs = GetConfig(configurationRoot, inspectorOptionsValue);
        if (inspectorOptionsValue.ConfigRenderer is null)
            return httpContext.Response.WriteAsJsonAsync(configs);

        return inspectorOptionsValue.ConfigRenderer.Invoke(httpContext, configs);
    }

    private static ConfigModel[] GetConfig(IConfigurationRoot configurationRoot, ConfigInspectorOptions options)
    {
        var allKeys = configurationRoot.AsEnumerable()
            .ToDictionary(x => x.Key, _ => false);
        var providers = GetConfigProviders(configurationRoot);
        var config = new ConfigModel[providers.Count];

        for (var i = providers.Count - 1; i >= 0; i--)
        {
            var provider = providers[i];
            config[i] = new ConfigModel
            {
                Provider = provider.ToString() ?? provider.GetType().Name,
                Items = GetConfig(provider, allKeys).ToArray()
            };
        }

        if (options.IncludeEmptyProviders)
        {
            return config;
        }

        return config.Where(x => x.Items is { Length: > 0 }).ToArray();
    }

    private static List<IConfigurationProvider> GetConfigProviders(IConfigurationRoot configurationRoot)
    {
        var providers = new List<IConfigurationProvider>();

        foreach (var provider in configurationRoot.Providers)
        {
            if (provider is not ChainedConfigurationProvider chainedConfigurationProvider)
            {
                providers.Add(provider);
                continue;
            }

#if NET7_0_OR_GREATER
            if (chainedConfigurationProvider.Configuration is not IConfigurationRoot chainsConfigurationRoot)
            {
                continue;
            }

            providers.AddRange(GetConfigProviders(chainsConfigurationRoot));
#endif
        }

        return providers;
    }

    private static IEnumerable<ConfigItemModel> GetConfig(IConfigurationProvider provider,
        Dictionary<string, bool> keys)
    {
        foreach (var (key, active) in keys)
        {
            if (!provider.TryGet(key, out var value))
                continue;

            var configItem = new ConfigItemModel { Key = key, Value = value };
            if (!active)
            {
                configItem.Active = keys[key] = true;
            }

            yield return configItem;
        }
    }
}
