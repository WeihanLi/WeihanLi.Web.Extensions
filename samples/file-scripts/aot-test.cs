// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/WeihanLi.Web.Extensions/
#:package Microsoft.AspNetCore.OpenApi
#:package Scalar.AspNetCore

using Scalar.AspNetCore;
using WeihanLi.Web.Authentication;
using WeihanLi.Web.Authentication.BasicAuthentication;
using WeihanLi.Web.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
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
    })
    ;
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapRuntimeInfo().ShortCircuit().DisableHttpMetrics();

var probes = app.MapProbes("/probes");
probes.MapGet("/live", () => Results.Ok());
probes.MapGet("/ready", () => Results.Ok());

app.Map("/basic-auth-test", () => "Hello").RequireAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();

await app.RunAsync();
