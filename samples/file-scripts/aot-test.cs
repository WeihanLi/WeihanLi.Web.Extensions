// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/WeihanLi.Web.Extensions/
#:package Microsoft.AspNetCore.OpenApi
#:package Scalar.AspNetCore

using Scalar.AspNetCore;
using System.Security.Claims;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.BasicAuthentication;
using WeihanLi.Web.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.UserName = "test";
        options.Password = "test";
        options.ClaimsGenerator = (context, opts) =>
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            return Task.FromResult<IReadOnlyCollection<Claim>>(claims);
        };
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
    })
    ;
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapRuntimeInfo().ShortCircuit().DisableHttpMetrics();
app.MapConfigInspector();

var probes = app.MapProbes("/probes");
probes.MapGet("/live", () => Results.Ok());
probes.MapGet("/ready", () => Results.Ok());

app.Map("/basic-auth-test", (HttpContext httpContext) => $"Hello {httpContext.User.Identity?.Name}({httpContext.User.GetUserId<int>()})").RequireAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();

await app.RunAsync();
