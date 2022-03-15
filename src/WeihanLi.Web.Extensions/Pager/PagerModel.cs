// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Pager;

internal sealed class PagerModel : IPagerModel
{
    public PagingDisplayMode PagingDisplayMode { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int PageCount { get; set; }

    public int TotalCount { get; set; }

    public PagerModel(int pageNumber, int pageSize, int totalCount)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        PageCount = Convert.ToInt32(Math.Ceiling(TotalCount * 1.0 / PageSize));
    }

    public bool IsFirstPage => PageNumber <= 1;

    public bool IsLastPage => PageNumber >= PageCount;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < PageCount;

    public int FirstItem => (PageNumber - 1) * PageSize + 1;

    public int LastItem
    {
        get
        {
            if (IsLastPage)
            {
                return FirstItem + (TotalCount - 1) % PageSize;
            }
            else
            {
                return PageNumber * PageSize;
            }
        }
    }

    public Func<int, string> OnPageChange { get; set; }

    private int groupSize = 12;

    public int GroupSize
    {
        get => groupSize;
        set
        {
            if (value > 1)
            {
                groupSize = value;
            }
        }
    }

    public bool ShowJumpButton { get; set; }
}
