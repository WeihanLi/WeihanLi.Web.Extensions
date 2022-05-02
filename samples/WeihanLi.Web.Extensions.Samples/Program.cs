// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Aspect;
using WeihanLi.Extensions;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.ApiKeyAuthentication;
using WeihanLi.Web.Authentication.HeaderAuthentication;
using WeihanLi.Web.Authorization.Jwt;
using WeihanLi.Web.Extensions;
using WeihanLi.Web.Extensions.Samples;
using WeihanLi.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(HeaderAuthenticationDefaults.AuthenticationSchema)
    .AddJwtBearer()
    .AddQuery(options =>
    {
        options.UserIdQueryKey = "uid";
    })
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
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpContextUserIdProvider(options =>
    {
        options.UserIdFactory = context => $"{context.GetUserIP()}";
    });
builder.Host.UseFluentAspectsServiceProviderFactory(options =>
    {
        options.InterceptAll()
          .With<EventPublishLogInterceptor>();
    });

var app = builder.Build();

app.Map("/Hello", () => "Hello Minimal API!").AddFilter<ApiResultFilter>();
app.Map("/HelloV2", Hello).AddFilter<ApiResultFilter>();
app.Map("/BadRequest", BadRequest).AddFilter<ApiResultFilter>();

app.UseHealthCheck();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();


string Hello() => "Hello Minimal API!";

IResult BadRequest() => Results.BadRequest();
