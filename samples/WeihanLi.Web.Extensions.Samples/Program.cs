// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using WeihanLi.Common;
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

builder.Services.AddAuthentication(HeaderAuthenticationDefaults.AuthenticationSchema)
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
    ;
builder.Services.AddJwtTokenServiceWithJwtBearerAuth(options =>
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
            new SymmetricSecurityKey(GuidIdGenerator.Instance.NewId().GetBytes()),
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
    options.InputFormatters.Add(new RawTextInputFormatter());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddHttpContextUserIdProvider(options =>
{
    options.UserIdFactory = context => $"{context.GetUserIP()}";
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "v1 API docs"
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), true);
});

builder.Host.UseFluentAspectsServiceProviderFactory(options =>
    {
        options.InterceptAll()
            .With<EventPublishLogInterceptor>();
    }, ignoreTypesPredict: t => t.HasNamespace() && (
        t.Namespace.StartsWith("Microsoft.")
        || t.Namespace.StartsWith("System.")
        || t.Namespace.StartsWith("Swashbuckle.")
        )
    );

var app = builder.Build();

app.MapRuntimeInfo();
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
app.UseSwagger().UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();


string Hello() => "Hello Minimal API!";

IResult BadRequest() => Results.BadRequest();
