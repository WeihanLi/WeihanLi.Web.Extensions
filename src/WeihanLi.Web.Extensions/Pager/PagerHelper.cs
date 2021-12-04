using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WeihanLi.Web.Pager
{
    /// <summary>
    /// PagerHelper 分页帮助类
    /// </summary>
    public static class PagerHelper
    {
        /// <summary>
        /// HtmlHelper Pager - 扩展方法
        /// </summary>
        /// <param name="helper">HtmlHelper</param>
        /// <param name="pagerModel">分页信息</param>
        /// <param name="onPageChange">翻页地址或事件</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, IPagerModel pagerModel, Func<int, string> onPageChange)
        {
            return helper.Pager(pagerModel, onPageChange, "_PagerPartial");
        }

        /// <summary>
        /// HtmlHelper Pager - 扩展方法
        /// </summary>
        /// <param name="helper">HtmlHelper</param>
        /// <param name="pagerModel">分页信息</param>
        /// <param name="onPageChange">翻页地址或事件</param>
        /// <param name="pagerViewName">分页分部视图名称，默认值为【_PagerPartial】</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, IPagerModel pagerModel, Func<int, string> onPageChange, string pagerViewName)
        {
            return helper.Pager(pagerModel, onPageChange, pagerViewName, PagingDisplayMode.Always);
        }

        /// <summary>
        /// HtmlHelper Pager - 扩展方法
        /// </summary>
        /// <param name="helper">HtmlHelper</param>
        /// <param name="pagerModel">分页信息</param>
        /// <param name="onPageChange">翻页地址或事件</param>
        /// <param name="pagerViewName">分页分部视图名称，默认值为【_PagerPartial】</param>
        /// <param name="displayMode">分页显示模式</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, IPagerModel pagerModel, Func<int, string> onPageChange, string pagerViewName, PagingDisplayMode displayMode)
        {
            pagerModel.OnPageChange = onPageChange;
            pagerModel.PagingDisplayMode = displayMode;
            return helper.Partial(pagerViewName, pagerModel);
        }
    }
}
