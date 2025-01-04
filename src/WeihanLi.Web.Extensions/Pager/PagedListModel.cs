// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Collections;

namespace WeihanLi.Web.Pager;

/// <summary>
/// PagedListModel
/// </summary>
/// <typeparam name="T">Type</typeparam>
internal sealed class PagedListModel<T> : IPagedListModel<T>
{
    public IReadOnlyList<T> Data { get; }

    public IPagerModel Pager { get; }

    public int Count => Data.Count;

    #nullable disable
    public PagedListModel(IEnumerable<T> data, IPagerModel pager)
    {
        Data = data?.ToArray() ?? [];
        Pager = pager;
    }
    #nullable restore

    public IEnumerator<T> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    public T this[int i]
    {
        get
        {
            if (i < 0 || i >= Data.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return Data[i];
        }
    }
}
