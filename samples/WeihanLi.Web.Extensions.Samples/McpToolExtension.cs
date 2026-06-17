// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO.Pipelines;
using System.Security.Claims;

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

public sealed class McpServerEndpointConfigureOptions(
    EndpointDataSource endpointDataSource,
    IApiDescriptionGroupCollectionProvider apiDescriptions,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services
    ) : IConfigureOptions<McpServerOptions>
{
    public void Configure(McpServerOptions options)
    {
        options.ToolCollection ??= new();

        var descriptions = apiDescriptions.ApiDescriptionGroups.Items
            .SelectMany(x => x.Items)
            .ToArray();

        foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            var metadata = endpoint.Metadata.GetMetadata<IMcpToolEndpointMetadata>();
            if (metadata is null || endpoint.RequestDelegate is null)
                continue;

            var apiDescription = descriptions.FirstOrDefault(x =>
                x.ActionDescriptor.EndpointMetadata?.Contains(metadata) == true);

            var descriptor = EndpointToolDescriptor.Create(endpoint, metadata, apiDescription);
            options.ToolCollection.Add(new EndpointMcpServerTool(httpContextAccessor, services, endpoint.RequestDelegate, descriptor));
        }
    }
}

file sealed class EndpointMcpServerTool(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services,
    RequestDelegate requestDelegate,
    EndpointToolDescriptor descriptor
    ) : McpServerTool
{
    public override Tool ProtocolTool { get; } = descriptor.CreateTool();

    public override IReadOnlyList<object> Metadata { get; } = [descriptor];

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> requestContext,
        CancellationToken cancellationToken = default)
    {
        var parentContext = httpContextAccessor.HttpContext;

        await using var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = parentContext?.RequestServices ?? services,
            User = parentContext?.User ?? new System.Security.Claims.ClaimsPrincipal()
        };
        httpContext.Response.Body = responseBody;

        var arguments = requestContext.Params?.Arguments;
        descriptor.ApplyRequest(httpContext, arguments);

        var originalContext = httpContextAccessor.HttpContext;
        httpContextAccessor.HttpContext = httpContext;
        try
        {
            await requestDelegate(httpContext);
        }
        catch (Exception ex)
        {
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = ex.Message }]
            };
        }
        finally
        {
            httpContextAccessor.HttpContext = originalContext;
        }

        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, Encoding.UTF8);
        var responseText = await reader.ReadToEndAsync(cancellationToken);

        return new CallToolResult
        {
            IsError = httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest,
            StructuredContent = TryParseJson(responseText),
            Content =
            [
                new TextContentBlock
                {
                    Text = string.IsNullOrEmpty(responseText)
                        ? $"HTTP {httpContext.Response.StatusCode}"
                        : responseText
                }
            ]
        };
    }

    private static JsonElement? TryParseJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

file sealed class EndpointToolDescriptor
{
    private static readonly JsonElement EmptyObjectSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new { }
    });

    private EndpointToolDescriptor(
        RouteEndpoint endpoint,
        string name,
        string description,
        string httpMethod,
        RoutePattern routePattern,
        IReadOnlyList<EndpointToolParameter> parameters,
        JsonElement inputSchema
        )
    {
        Endpoint = endpoint;
        Name = name;
        Description = description;
        HttpMethod = httpMethod;
        RoutePattern = routePattern;
        Parameters = parameters;
        InputSchema = inputSchema;
    }

    public RouteEndpoint Endpoint { get; }

    public string Name { get; }

    public string Description { get; }

    public string HttpMethod { get; }

    public RoutePattern RoutePattern { get; }

    public IReadOnlyList<EndpointToolParameter> Parameters { get; }

    public JsonElement InputSchema { get; }

    public static EndpointToolDescriptor Create(
        RouteEndpoint endpoint,
        IMcpToolEndpointMetadata metadata,
        ApiDescription? apiDescription)
    {
        var httpMethod = apiDescription?.HttpMethod
            ?? endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault()
            ?? HttpMethods.Get;
        var pattern = endpoint.RoutePattern;
        var path = pattern.RawText ?? endpoint.DisplayName ?? "/";
        var name = !string.IsNullOrWhiteSpace(metadata.Name)
            ? metadata.Name
            : endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
              ?? NormalizeToolName($"{httpMethod}_{path}");
        var description = !string.IsNullOrWhiteSpace(metadata.Description)
            ? metadata.Description
            : endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description
              ?? endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary
              ?? $"{httpMethod} {path}";

        var parameters = CreateParameters(endpoint, pattern, apiDescription);

        return new EndpointToolDescriptor(
            endpoint,
            NormalizeToolName(name),
            description,
            httpMethod,
            pattern,
            parameters,
            CreateInputSchema(parameters)
            );
    }

    public Tool CreateTool()
    {
        return new Tool
        {
            Name = Name,
            Description = Description,
            InputSchema = InputSchema
        };
    }

    public void ApplyRequest(HttpContext httpContext, IDictionary<string, JsonElement>? arguments)
    {
        var args = arguments ?? new Dictionary<string, JsonElement>();
        httpContext.SetEndpoint(Endpoint);
        httpContext.Request.Method = HttpMethod;
        httpContext.Request.Path = BuildPath(args);
        httpContext.Request.RouteValues = BuildRouteValues(args);
        httpContext.Request.QueryString = BuildQueryString(args);

        foreach (var parameter in Parameters.Where(x => x.Source == EndpointToolParameterSource.Header))
        {
            if (args.TryGetValue(parameter.Name, out var value))
            {
                httpContext.Request.Headers[parameter.Name] = value.ToString();
            }
        }

        ApplyFormRequest(httpContext, args);

        var bodyParameter = Parameters.FirstOrDefault(x => x.Source == EndpointToolParameterSource.Body);
        if (bodyParameter is not null)
        {
            var body = args.TryGetValue("body", out var bodyValue) || args.TryGetValue(bodyParameter.Name, out bodyValue)
                ? bodyValue
                : default;

            if (body.ValueKind is not JsonValueKind.Undefined)
            {
                var bytes = Encoding.UTF8.GetBytes(body.GetRawText());
                httpContext.Request.Body = new MemoryStream(bytes);
                httpContext.Request.ContentLength = bytes.Length;
                httpContext.Request.ContentType = "application/json";
            }
        }
    }

    private void ApplyFormRequest(HttpContext httpContext, IDictionary<string, JsonElement> arguments)
    {
        var formParameters = Parameters.Where(x => x.Source == EndpointToolParameterSource.Form && arguments.ContainsKey(x.Name)).ToArray();
        if (formParameters.Length == 0)
            return;

        var form = string.Join("&", formParameters.Select(x =>
            $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(arguments[x.Name].ToString())}"));
        var bytes = Encoding.UTF8.GetBytes(form);
        httpContext.Request.Body = new MemoryStream(bytes);
        httpContext.Request.ContentLength = bytes.Length;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
    }

    private PathString BuildPath(IDictionary<string, JsonElement> arguments)
    {
        var path = RoutePattern.RawText ?? "/";
        foreach (var routeParameter in RoutePattern.Parameters)
        {
            if (!arguments.TryGetValue(routeParameter.Name, out var value))
            {
                if (routeParameter.IsOptional)
                {
                    path = RemoveRouteToken(path, routeParameter.Name);
                    continue;
                }

                throw new ArgumentException($"Missing route parameter '{routeParameter.Name}'.");
            }

            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                if (routeParameter.IsOptional)
                {
                    path = RemoveRouteToken(path, routeParameter.Name);
                    continue;
                }

                throw new ArgumentException($"Route parameter '{routeParameter.Name}' is required.");
            }

            var routeValue = Uri.EscapeDataString(value.ToString());
            path = ReplaceRouteToken(path, routeParameter.Name, routeValue);
        }

        foreach (var routeParameter in RoutePattern.Parameters.Where(x => x.IsOptional))
        {
            if (arguments.ContainsKey(routeParameter.Name))
                continue;

            path = RemoveRouteToken(path, routeParameter.Name);
        }

        return path.StartsWith('/')
            ? path
            : $"/{path}";
    }

    private static string ReplaceRouteToken(string path, string parameterName, string value)
    {
        return RouteTokenRegex(parameterName).Replace(path, value);
    }

    private static string RemoveRouteToken(string path, string parameterName)
    {
        path = Regex.Replace(path, $"/{RouteTokenPattern(parameterName)}", string.Empty, RegexOptions.IgnoreCase);
        return RouteTokenRegex(parameterName).Replace(path, string.Empty);
    }

    private static Regex RouteTokenRegex(string parameterName)
    {
        return new Regex(RouteTokenPattern(parameterName), RegexOptions.IgnoreCase);
    }

    private static string RouteTokenPattern(string parameterName)
    {
        return @"\{" + Regex.Escape(parameterName) + @"(?:[:=][^}]+)?\??\}";
    }

    private RouteValueDictionary BuildRouteValues(IDictionary<string, JsonElement> arguments)
    {
        var values = new RouteValueDictionary();
        foreach (var parameter in Parameters.Where(x => x.Source == EndpointToolParameterSource.Path))
        {
            if (arguments.TryGetValue(parameter.Name, out var value))
            {
                values[parameter.Name] = value.ToString();
            }
        }
        return values;
    }

    private QueryString BuildQueryString(IDictionary<string, JsonElement> arguments)
    {
        var query = Parameters
            .Where(x => x.Source == EndpointToolParameterSource.Query && arguments.ContainsKey(x.Name))
            .Select(x => $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(arguments[x.Name].ToString())}");

        var queryString = string.Join("&", query);
        return string.IsNullOrEmpty(queryString)
            ? QueryString.Empty
            : new QueryString($"?{queryString}");
    }

    private static JsonElement CreateInputSchema(IReadOnlyList<EndpointToolParameter> parameters)
    {
        if (parameters.Count == 0)
            return EmptyObjectSchema;

        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var parameter in parameters)
        {
            var propertyName = parameter.Source == EndpointToolParameterSource.Body ? "body" : parameter.Name;
            properties[propertyName] = parameter.CreateJsonSchema();
            if (parameter.IsRequired)
            {
                required.Add(propertyName);
            }
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties
        };
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        return JsonSerializer.SerializeToElement(schema);
    }

    private static EndpointToolParameter[] CreateParameters(RouteEndpoint endpoint, RoutePattern pattern, ApiDescription? apiDescription)
    {
        var parameters = new Dictionary<string, EndpointToolParameter>(StringComparer.OrdinalIgnoreCase);
        foreach (var parameter in endpoint.Metadata
                     .OfType<IParameterBindingMetadata>()
                     .SelectMany(x => EndpointToolParameter.Create(x, pattern)))
        {
            parameters.TryAdd(parameter.Name, parameter);
        }

        if (apiDescription is not null)
        {
            foreach (var parameter in apiDescription.ParameterDescriptions
                         .Select(EndpointToolParameter.Create)
                         .Where(x => x is not null)
                         .Cast<EndpointToolParameter>())
            {
                if (!parameters.TryGetValue(parameter.Name, out var existing))
                {
                    parameters[parameter.Name] = parameter;
                    continue;
                }

                parameters[parameter.Name] = existing.WithDescription(parameter.Description);
            }
        }

        foreach (var routeParameter in pattern.Parameters)
        {
            if (parameters.ContainsKey(routeParameter.Name))
                continue;

            parameters[routeParameter.Name] = EndpointToolParameter.CreateRouteParameter(routeParameter);
        }

        return parameters.Values.ToArray();
    }

    private static string NormalizeToolName(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch is '_' or '-'
                ? ch
                : '_');
        }

        var name = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(name)
            ? "endpoint_tool"
            : name;
    }
}

file sealed class EndpointToolParameter
{
    private static readonly NullabilityInfoContext NullabilityInfoContext = new();

    private EndpointToolParameter(string name, Type type, EndpointToolParameterSource source, bool isRequired, string? description)
    {
        Name = name;
        Type = type;
        Source = source;
        IsRequired = isRequired;
        Description = description;
    }

    public string Name { get; }

    public Type Type { get; }

    public EndpointToolParameterSource Source { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public static IEnumerable<EndpointToolParameter> Create(IParameterBindingMetadata metadata, RoutePattern pattern)
    {
        var parameter = metadata.ParameterInfo;
        if (ShouldIgnoreParameter(parameter))
            yield break;

        if (IsAsParameters(parameter))
        {
            foreach (var toolParameter in CreateFromAsParameters(parameter.ParameterType, pattern))
            {
                yield return toolParameter;
            }
            yield break;
        }

        var source = ResolveSource(parameter, metadata, pattern);
        if (source is null)
            yield break;

        var name = ResolveName(parameter, metadata.Name, source.Value);
        yield return new EndpointToolParameter(
            name,
            parameter.ParameterType,
            source.Value,
            !metadata.IsOptional && !IsNullable(parameter),
            null
            );
    }

    public static EndpointToolParameter? Create(ApiParameterDescription parameter)
    {
        var source = parameter.Source switch
        {
            var s when s == BindingSource.Path => EndpointToolParameterSource.Path,
            var s when s == BindingSource.Query => EndpointToolParameterSource.Query,
            var s when s == BindingSource.Header => EndpointToolParameterSource.Header,
            var s when s == BindingSource.Body => EndpointToolParameterSource.Body,
            _ => (EndpointToolParameterSource?)null
        };

        return source is null
            ? null
            : new EndpointToolParameter(
                parameter.Name,
                parameter.Type,
                source.Value,
                parameter.IsRequired,
                parameter.ModelMetadata?.Description
                );
    }

    public EndpointToolParameter WithDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? this
            : new EndpointToolParameter(Name, Type, Source, IsRequired, description);
    }

    public static EndpointToolParameter CreateRouteParameter(RoutePatternParameterPart parameter)
    {
        return new EndpointToolParameter(
            parameter.Name,
            typeof(string),
            EndpointToolParameterSource.Path,
            !parameter.IsOptional,
            $"Route parameter '{parameter.Name}'."
            );
    }

    public JsonObject CreateJsonSchema()
    {
        var schema = CreateSchema(Type);
        if (!string.IsNullOrWhiteSpace(Description))
        {
            schema["description"] = Description;
        }
        return schema;
    }

    private static JsonObject CreateSchema(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return new JsonObject { ["type"] = "string" };
        if (type == typeof(bool))
            return new JsonObject { ["type"] = "boolean" };
        if (type.IsEnum)
            return new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray(type.GetEnumNames().Select(x => JsonValue.Create(x)).ToArray<JsonNode?>())
            };
        if (IsInteger(type))
            return new JsonObject { ["type"] = "integer" };
        if (IsNumber(type))
            return new JsonObject { ["type"] = "number" };

        return new JsonObject { ["type"] = "object" };
    }

    private static IEnumerable<EndpointToolParameter> CreateFromAsParameters(Type type, RoutePattern pattern)
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length != 0)
                continue;

            if (property.GetCustomAttributes().OfType<IFromServiceMetadata>().Any())
                continue;

            var source = ResolveSource(property, pattern);
            if (source is null)
                continue;

            var name = ResolveName(property, property.Name, source.Value);
            yield return new EndpointToolParameter(
                name,
                property.PropertyType,
                source.Value,
                !IsNullable(property),
                null
                );
        }
    }

    private static EndpointToolParameterSource? ResolveSource(ParameterInfo parameter, IParameterBindingMetadata metadata, RoutePattern pattern)
    {
        var explicitSource = ResolveSource(parameter);
        if (explicitSource is not null)
            return explicitSource;

        var name = metadata.Name ?? parameter.Name;
        if (!string.IsNullOrWhiteSpace(name) && pattern.Parameters.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            return EndpointToolParameterSource.Path;

        if (metadata.HasTryParse || IsSimpleType(parameter.ParameterType))
            return EndpointToolParameterSource.Query;

        if (metadata.HasBindAsync)
            return null;

        return EndpointToolParameterSource.Body;
    }

    private static EndpointToolParameterSource? ResolveSource(ICustomAttributeProvider attributeProvider, RoutePattern? pattern = null)
    {
        var attributes = attributeProvider.GetCustomAttributes(false);
        if (attributes.OfType<IFromServiceMetadata>().Any())
            return null;
        if (attributes.OfType<IFromRouteMetadata>().Any())
            return EndpointToolParameterSource.Path;
        if (attributes.OfType<IFromQueryMetadata>().Any())
            return EndpointToolParameterSource.Query;
        if (attributes.OfType<IFromHeaderMetadata>().Any())
            return EndpointToolParameterSource.Header;
        if (attributes.OfType<IFromFormMetadata>().Any())
            return EndpointToolParameterSource.Form;
        if (attributes.OfType<IFromBodyMetadata>().Any())
            return EndpointToolParameterSource.Body;

        if (attributeProvider is PropertyInfo property)
        {
            if (pattern is not null && pattern.Parameters.Any(x => string.Equals(x.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
                return EndpointToolParameterSource.Path;

            return IsSimpleType(property.PropertyType)
                ? EndpointToolParameterSource.Query
                : EndpointToolParameterSource.Body;
        }

        return null;
    }

    private static string ResolveName(ICustomAttributeProvider attributeProvider, string? fallback, EndpointToolParameterSource source)
    {
        var attributes = attributeProvider.GetCustomAttributes(false);
        var name = source switch
        {
            EndpointToolParameterSource.Path => attributes.OfType<IFromRouteMetadata>().FirstOrDefault()?.Name,
            EndpointToolParameterSource.Query => attributes.OfType<IFromQueryMetadata>().FirstOrDefault()?.Name,
            EndpointToolParameterSource.Header => attributes.OfType<IFromHeaderMetadata>().FirstOrDefault()?.Name,
            EndpointToolParameterSource.Form => attributes.OfType<IFromFormMetadata>().FirstOrDefault()?.Name,
            _ => null
        };

        return string.IsNullOrWhiteSpace(name)
            ? fallback ?? "value"
            : name;
    }

    private static bool ShouldIgnoreParameter(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        return type == typeof(HttpContext)
               || type == typeof(HttpRequest)
               || type == typeof(HttpResponse)
               || type == typeof(ClaimsPrincipal)
               || type == typeof(CancellationToken)
               || type == typeof(Stream)
               || type == typeof(PipeReader)
               || typeof(IFormFile).IsAssignableFrom(type)
               || typeof(IFormFileCollection).IsAssignableFrom(type)
               || parameter.GetCustomAttributes().OfType<IFromServiceMetadata>().Any();
    }

    private static bool IsAsParameters(ParameterInfo parameter)
    {
        return parameter.GetCustomAttributes().Any(x => x.GetType().Name == "AsParametersAttribute");
    }

    private static bool IsNullable(ParameterInfo parameter)
    {
        if (Nullable.GetUnderlyingType(parameter.ParameterType) is not null)
            return true;
        if (parameter.HasDefaultValue)
            return true;
        if (!parameter.ParameterType.IsValueType)
            return NullabilityInfoContext.Create(parameter).ReadState == NullabilityState.Nullable;
        return false;
    }

    private static bool IsNullable(PropertyInfo property)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) is not null)
            return true;
        if (!property.PropertyType.IsValueType)
            return NullabilityInfoContext.Create(property).ReadState == NullabilityState.Nullable;
        return false;
    }

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(Guid)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(DateOnly)
               || type == typeof(TimeOnly)
               || type == typeof(decimal)
               || type == typeof(Uri);
    }

    private static bool IsInteger(Type type)
    {
        return type == typeof(byte)
               || type == typeof(sbyte)
               || type == typeof(short)
               || type == typeof(ushort)
               || type == typeof(int)
               || type == typeof(uint)
               || type == typeof(long)
               || type == typeof(ulong);
    }

    private static bool IsNumber(Type type)
    {
        return type == typeof(float)
               || type == typeof(double)
               || type == typeof(decimal);
    }
}

file enum EndpointToolParameterSource
{
    Path,
    Query,
    Header,
    Form,
    Body
}

public static class McpToolExtension
{
    public static IMcpServerBuilder WithEndpointTools(this IMcpServerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
            if (string.IsNullOrWhiteSpace(metadata.Name))
            {
                metadata.Name = c.Metadata.OfType<IEndpointNameMetadata>().LastOrDefault()?.EndpointName
                                ?? c.DisplayName
                                ?? "endpoint_tool";
            }
            if (string.IsNullOrWhiteSpace(metadata.Description))
            {
                metadata.Description = c.Metadata.OfType<IEndpointDescriptionMetadata>().LastOrDefault()?.Description
                                       ?? c.Metadata.OfType<IEndpointSummaryMetadata>().LastOrDefault()?.Summary
                                       ?? c.DisplayName
                                       ?? metadata.Name;
            }

            c.Metadata.Add(metadata);
        });
        return builder;
    }
}
