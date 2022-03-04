// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace WeihanLi.Web.AccessControlHelper;

public static class HtmlHelperExtension
{
    /// <summary>
    /// SparkActionLink
    /// </summary>
    /// <param name="helper">HtmlHelper</param>
    /// <param name="linkText">linkText</param>
    /// <param name="actionName">actionName</param>
    /// <param name="controllerName">controllerName</param>
    /// <param name="routeValues">routeValues</param>
    /// <param name="htmlAttributes">htmlAttributes</param>
    /// <param name="accessKey">accessKey</param>
    /// <returns></returns>
    public static IHtmlContent SparkActionLink(this IHtmlHelper helper, string linkText, string actionName, string controllerName = "", object routeValues = null, object htmlAttributes = null, string accessKey = "")
    {
        if (helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IControlAccessStrategy>()
                        .IsControlCanAccess(accessKey))
        {
            if (string.IsNullOrEmpty(controllerName))
            {
                return helper.ActionLink(linkText, actionName, routeValues, htmlAttributes);
            }
            else
            {
                return helper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }
        }
        return HtmlString.Empty;
    }

    /// <summary>
    /// SparkContainer
    /// </summary>
    /// <param name="helper">HtmlHelper</param>
    /// <param name="tagName">tagName</param>
    /// <param name="attributes">htmlAttributes</param>
    /// <param name="accessKey">accessKey</param>
    /// <returns></returns>
    public static SparkContainer SparkContainer(this IHtmlHelper helper, string tagName, object attributes = null, string accessKey = null)
    => SparkContainerHelper(helper, tagName, HtmlHelper.AnonymousObjectToHtmlAttributes(attributes), accessKey);

    private static SparkContainer SparkContainerHelper(IHtmlHelper helper, string tagName,
        IDictionary<string, object> attributes = null, string accessKey = null)
    {
        var tagBuilder = new TagBuilder(tagName);
        var canAccess = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IControlAccessStrategy>()
                        .IsControlCanAccess(accessKey);
        if (canAccess)
        {
            tagBuilder.MergeAttributes(attributes);
            tagBuilder.TagRenderMode = TagRenderMode.StartTag;
            helper.ViewContext.Writer.Write(tagBuilder);
        }
        return new SparkContainer(helper.ViewContext, tagName, canAccess);
    }
}
