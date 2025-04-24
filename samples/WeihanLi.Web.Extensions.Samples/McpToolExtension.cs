// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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

public static class McpToolExtension
{
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
