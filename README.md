# WeihanLi.Web.Extensions

> A curated set of practical building blocks for ASP.NET Core apps: authentication providers, endpoint filters, middleware, MVC helpers, and more.

[![NuGet](https://img.shields.io/nuget/v/WeihanLi.Web.Extensions)](https://www.nuget.org/packages/WeihanLi.Web.Extensions/) 
[![NuGet (prerelease)](https://img.shields.io/nuget/vpre/WeihanLi.Web.Extensions)](https://www.nuget.org/packages/WeihanLi.Web.Extensions/absoluteLatest) 
[![Azure Pipelines](https://weihanli.visualstudio.com/Pipelines/_apis/build/status/WeihanLi.WeihanLi.Web.Extensions?branchName=dev)](https://weihanli.visualstudio.com/Pipelines/_build/latest?definitionId=19&branchName=dev) 
[![GitHub Actions](https://github.com/WeihanLi/WeihanLi.Web.Extensions/workflows/dotnetcore/badge.svg)](https://github.com/WeihanLi/WeihanLi.Web.Extensions/actions?query=workflow%3Adotnetcore)

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Highlights](#usage-highlights)
- [Samples and Docs](#samples-and-docs)
- [Build Locally](#build-locally)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Authentication providers** – drop-in handlers for API key, basic, header, query string, and delegate-based schemes (`AuthenticationBuilderExtension`).
- **Access control helper** – configure granular resource checks, tag helpers, and middleware, or promote the policy to the app default (`AccessControlHelper`).
- **JWT tooling** – lightweight `JwtTokenService` for issuing/refreshing tokens with ASP.NET Core authentication integration.
- **Response filters** – `ApiResultFilter`, environment/conditional filters, and feature-flag-aware filters for consistent API responses.
- **Middleware toolset** – exception handler, configuration inspector, feature-flag-based pipeline branching, health check helpers.
- **MVC utilities** – strongly typed pager, plain-text formatter, tenant/user providers, and HTTP context extensions (`HttpContextExtension`). 
- **Target frameworks** – ships for `net8.0`, `net9.0`, and `net10.0`.

Explore the `src/WeihanLi.Web.Extensions` folder for the full catalog; each namespace groups related functionality (Authentication, Authorization, Filters, Middleware, Pager, etc.).

## Installation

```bash
dotnet add package WeihanLi.Web.Extensions
```

## Quick Start

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiResultFilter>(); // standardize API responses
    })
    .AddJsonOptions();

builder.Services.AddAuthentication()
    .AddApiKey(options =>
    {
        options.ApiKeyName = "X-ApiKey";
        options.ApiKey = "super-secret-key";
    })
    .AddBasic(options =>
    {
        options.UserName = "demo";
        options.Password = "demo";
    });

builder.Services.AddAccessControlHelper<MyResourceStrategy, MyControlStrategy>(options =>
{
    options.UseAsDefaultPolicy = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAccessControlHelper(); // enable middleware support

app.MapControllers();
app.Run();
```

Replace `MyResourceStrategy`/`MyControlStrategy` with your own `IResourceAccessStrategy`/`IControlAccessStrategy` implementations. For a more complete walk-through, review the sample project linked below.

## Usage Highlights

- **Access control** – decorate MVC controllers with `[AccessControl]`, register tag helpers, or use `UseAccessControlHelper()` middleware to enforce centralised policies.
- **API response shaping** – apply `ApiResultFilter` globally, per controller, or as a minimal API endpoint filter to wrap responses with the built-in `Result` model.
- **Feature flags** – gate middleware with `app.UseIfFeatureEnabled(...)` or guard actions via `[FeatureFlagFilter("MyFlag")]`, using configuration-driven toggles.
- **Authentication mix-and-match** – compose multiple schemes (`AddApiKey`, `AddHeader`, `AddQuery`, `AddDelegate`) and map policies using standard `AuthorizeAttribute`.
- **Developer tooling** – surface configuration via `app.MapConfigInspector()`, enrich structured logging with `HttpContextLoggingEnricher`, or expose probes with `app.MapProbes(...)`.

These helpers are additive: you can adopt a single filter or mix several features without impacting the default ASP.NET Core pipeline.

## Samples and Docs

- Sample application: `samples/WeihanLi.Web.Extensions.Samples` demonstrates authentication combos, endpoint filters, feature switches, and the config inspector UI.
- Release notes: `docs/ReleaseNotes.md` tracks major additions for each NuGet version.
- API reference / documentation site: build with `docfx docfx.json` or browse the published GitHub Pages site (if available).

## Build Locally

> Have the latest .NET SDK installed first, download from <https://get.dot.net/>

Build the project

```bash
dotnet build
```

To experiment quickly, run the sample:

```bash
dotnet run --project samples/WeihanLi.Web.Extensions.Samples
```

## Contributing

Contributions are welcome—issues, feature ideas, and pull requests all help the project grow. Please discuss sizeable changes in an issue before opening a PR and ensure builds pass locally (`dotnet build`) before submitting.

## License

This project is licensed under the [Apache License 2.0](LICENSE).
