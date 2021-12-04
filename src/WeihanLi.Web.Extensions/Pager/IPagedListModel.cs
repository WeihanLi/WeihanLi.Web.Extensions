﻿using System.Collections.Generic;

namespace WeihanLi.Web.Pager
{
    /// <summary>
    /// IPagedListModel
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public interface IPagedListModel<out T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Data
        /// </summary>
        IReadOnlyList<T> Data { get; }

        /// <summary>
        /// PagerModel
        /// </summary>
        IPagerModel Pager { get; }
    }
}
