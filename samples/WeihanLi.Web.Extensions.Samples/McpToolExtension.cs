// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelContextProtocol.Server;
using System.Diagnostics;

namespace WeihanLi.Web.Extensions.Samples;

public interface IMcpToolEndpointMetadata
{
    string Name { get; set; }
    string Description { get; set; }
}

public class McpToolEndpointMetadata : IMcpToolEndpointMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class McpServerEndpointConfigureOptions(EndpointDataSource endpointDataSource, IServiceProvider services) : IConfigureOptions<McpServerOptions>
{
    public void Configure(McpServerOptions options)
    {
        options.Capabilities ??= new();
        options.Capabilities.Tools.ToolCollection ??= new ();

        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (!endpoint.Metadata.Any(m => m is IMcpToolEndpointMetadata))
                continue;
            
            Debug.Assert(endpoint.RequestDelegate is not null);
            
            var tool = McpServerTool.Create(endpoint.RequestDelegate.Method, typeof(HttpContext), new McpServerToolCreateOptions
            {
                Services = services
            });
            options.Capabilities.Tools.ToolCollection.Add(tool);
        }
    }
}

public static class McpToolExtension
{
    public static IMcpServerBuilder WithEndpointTools(this IMcpServerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<McpServerOptions>, McpServerEndpointConfigureOptions>());
        return builder;
    }
    
    public static IEndpointConventionBuilder AsMcpTool<TBuilder>(this TBuilder builder, Action<McpToolEndpointMetadata>? toolConfigure = null)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        var metadata = new McpToolEndpointMetadata();
        toolConfigure?.Invoke(metadata);
        builder.Add(c =>
        {
            if (string.IsNullOrEmpty(metadata.Name))
            {
                metadata.Name = c.DisplayName!;
            }
            if (string.IsNullOrEmpty(metadata.Description))
            {
                metadata.Description = c.DisplayName!;
            }
            c.Metadata.Add(metadata);
        });
        return builder;
    }
}

internal sealed class McpInvocation(IHttpContextAccessor contextAccessor)
{
    
}
