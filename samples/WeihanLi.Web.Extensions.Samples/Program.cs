// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using WeihanLi.Common.Aspect;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authorization.Jwt;
using WeihanLi.Web.Extensions;
using WeihanLi.Web.Extensions.Samples;
using WeihanLi.Web.Filters;
using WeihanLi.Web.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(HeaderAuthenticationDefaults.AuthenticationScheme)
    .AddJwtBearer()
    .AddBasic(options =>
    {
        options.UserName = "test";
        options.Password = "test";
    })
    .AddQuery(options => { options.UserIdQueryKey = "uid"; })
    .AddHeader(options =>
    {
        options.UserIdHeaderName = "X-UserId";
        options.UserNameHeaderName = "X-UserName";
        options.UserRolesHeaderName = "X-UserRoles";
    })
    .AddApiKey(options =>
    {
        options.ClaimsIssuer = "https://id.weihanli.xyz";
        options.ApiKey = "123456";
        options.ApiKeyName = "X-ApiKey";
        options.KeyLocation = KeyLocation.HeaderOrQuery;
    })
    .AddDelegate(options =>
    {
        options.Validator = c => (c.Request.Headers.TryGetValue("x-delegate-key", out var values) && values.ToString().Equals("test"))
            .WrapTask();
        options.ClaimsGenerator = c =>
        {
            Claim[] claims =
            [
                new (ClaimTypes.Name, "test")
            ];
            return Task.FromResult<IReadOnlyCollection<Claim>>(claims);
        };
    })
    ;
builder.Services.AddJwtServiceWithJwtBearerAuth(options =>
{
    options.SecretKey = Guid.NewGuid().ToString();
    options.Issuer = "https://id.weihanli.xyz";
    options.Audience = "SparkTodo";
    // EnableRefreshToken, disabled by default
    options.EnableRefreshToken = true;
    // Renew refresh token always
    // options.RenewRefreshTokenPredicate = _ => true;
    options.RefreshTokenSigningCredentialsFactory = () =>
        new SigningCredentials(
            new SymmetricSecurityKey(WeihanLi.Common.Services.GuidIdGenerator.Instance.NewId().GetBytes()),
            SecurityAlgorithms.HmacSha256
        );
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Basic",
        policyBuilder => policyBuilder.AddAuthenticationSchemes("Basic").RequireAuthenticatedUser());
});

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new PlainTextInputFormatter());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddHttpContextUserIdProvider(options =>
{
    options.UserIdFactory = static context => $"{context.GetUserIP()}";
});

// builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Host.UseFluentAspectsServiceProviderFactory(options =>
    {
        options.InterceptAll()
            .With<EventPublishLogInterceptor>();
    }, ignoreTypesPredict: t => t.HasNamespace() && (
        t.Namespace!.StartsWith("Microsoft.")
        || t.Namespace.StartsWith("System.")
        || t.Namespace.StartsWith("Swashbuckle.")
        )
    );

var app = builder.Build();

app.MapRuntimeInfo().ShortCircuit();
app.Map("/Hello", () => "Hello Minimal API!").AddEndpointFilter<ApiResultFilter>();
app.Map("/HelloV2", Hello).AddEndpointFilter<ApiResultFilter>();
app.Map("/HelloV3", () => Results.Ok(new { Name = "test" })).AddEndpointFilter<ApiResultFilter>();
app.Map("/HelloV4", () => Results.Ok(Result.Success(new { Name = "test" }))).AddEndpointFilter<ApiResultFilter>();
app.Map("/BadRequest", BadRequest).AddEndpointFilter<ApiResultFilter>();
app.Map("/basic-auth-test", () => "Hello").RequireAuthorization("Basic");

// conditional filter
var conditionalTest = app.MapGroup("/conditional");
conditionalTest.Map("/NotFound", () => "Not Found")
    .AddEndpointFilter(new EnvironmentFilter("Production"));
conditionalTest.Map("/Dynamic", () => "You get it")
    .AddEndpointFilter(new ConditionalFilter()
    {
        ConditionFunc = c => c.Request.Query.TryGetValue("enable", out _),
        ResultFactory = c => Results.NotFound(new { c.Request.QueryString })
    });

var testGroup1 = app.MapGroup("/test1");
testGroup1.Map("/hello", () => "Hello");
testGroup1.Map("/world", () => "World");
testGroup1.AddEndpointFilter<ApiResultFilter>();


var envGroup = app.MapGroup("/env-test");
envGroup.Map("/dev", () => "env-test")
    .AddEndpointFilter(new EnvironmentFilter(Environments.Development));
envGroup.Map("/prod", () => "env-test")
    .AddEndpointFilter(new EnvironmentFilter(Environments.Production));
// attribute endpoint filter not supported for now, https://github.com/dotnet/aspnetcore/issues/43421
// envGroup.Map("/stage", [EnvironmentFilter("Staging")]() => "env-test");

app.UseHealthCheck();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

// app.MapConfigInspector(optionsConfigure: options =>
// {
//     options.ConfigRenderer = async (context, configs) =>
//     {
//         var htmlStart = """
//                         <html>
//                           <head>
//                             <title>Config Inspector</title>
//                           </head>
//                           <body>
//                             <table style="font-size:1.2em;line-height:1.6em">
//                               <thead>
//                               <tr>
//                                 <th>Provider</th>
//                                 <th>Key</th>
//                                 <th>Value</th>
//                                 <th>Active</th>
//                               </tr>
//                               </thead>
//                               <tbody>
//                         """;
//         var htmlEnd = "</tbody></table></body></html>";
//         var tbody = new StringBuilder();
//         foreach (var config in configs)
//         {
//             tbody.Append($"<tr><td>{config.Provider}</td>");
//             foreach (var item in config.Items)
//             {
//                 tbody.Append(
//                     $$"""<td>{{item.Key}}</td><td>{{item.Value}}</td><td><input type="checkbox" {{(item.Active ? "checked" : "")}} /></td>""");
//             }
//
//             tbody.AppendLine("</tr>");
//         }
//
//         var responseText = $"{htmlStart}{tbody}{htmlEnd}";
//         await context.Response.WriteAsync(responseText);
//     };
// });

app.MapConfigInspector()
    // .RequireAuthorization(x => x
    //     .AddAuthenticationSchemes("ApiKey")
    //     .RequireAuthenticatedUser()
    // )
    ;
app.MapControllers();

await app.RunAsync();


static string Hello() => "Hello Minimal API!";

static IResult BadRequest() => Results.BadRequest();
