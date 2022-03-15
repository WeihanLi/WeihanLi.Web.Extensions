// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Pager;

public static class PagedListModelExtension
{
    public static IPagedListModel<T> ToPagedList<T>(this IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
    {
        return new PagedListModel<T>(data, new PagerModel(pageNumber, pageSize, totalCount));
    }

    public static IPagedListModel<T> ToPagedList<T>(this Common.Models.IPagedListResult<T> data)
    {
        return new PagedListModel<T>(data.Data, new PagerModel(data.PageNumber, data.PageSize, data.TotalCount));
    }

    public static IPagedListModel<T> ToPagedList<T>(this Common.Models.IListResultWithTotal<T> data, int pageNum, int pageSize)
    {
        return new PagedListModel<T>(data.Data, new PagerModel(pageNum, pageSize, data.TotalCount));
    }
}
