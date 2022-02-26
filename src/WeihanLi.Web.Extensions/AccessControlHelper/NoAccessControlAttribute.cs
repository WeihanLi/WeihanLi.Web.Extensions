
/* Unmerged change from project 'WeihanLi.Web.Extensions(net5.0)'
Before:
using System;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System;
*/

/* Unmerged change from project 'WeihanLi.Web.Extensions(netcoreapp3.1)'
Before:
using System;
After:
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System;
*/
// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using System;

/* Unmerged change from project 'WeihanLi.Web.Extensions(net5.0)'
Before:
namespace WeihanLi.Web.AccessControlHelper
{
    /// <summary>
    /// NoAccessControl
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
        }
After:
namespace WeihanLi.Web.AccessControlHelper;

/// <summary>
/// NoAccessControl
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
*/

/* Unmerged change from project 'WeihanLi.Web.Extensions(netcoreapp3.1)'
Before:
namespace WeihanLi.Web.AccessControlHelper
{
    /// <summary>
    /// NoAccessControl
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
        }
After:
namespace WeihanLi.Web.AccessControlHelper;

/// <summary>
/// NoAccessControl
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
*/

namespace WeihanLi.Web.AccessControlHelper;

/// <summary>
/// NoAccessControl
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoAccessControlAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
    }
}
