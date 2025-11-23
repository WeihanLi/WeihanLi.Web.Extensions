// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;
using WeihanLi.Web.Middleware;

namespace WeihanLi.Web.Internals;

[JsonSerializable(typeof(ConfigModel))]
[JsonSerializable(typeof(ConfigModel[]))]
[JsonSerializable(typeof(ConfigItemModel))]
[JsonSerializable(typeof(ConfigItemModel[]))]
internal partial class CustomJsonContext : JsonSerializerContext
{
}
