// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Rendering;

namespace WeihanLi.Web.AccessControlHelper;

public sealed class SparkContainer : IDisposable
{
    private readonly string _tagName;
    private readonly ViewContext _viewContext;
    private bool _disposed;
    private TextWriter? _writer;

    public SparkContainer(ViewContext viewContext, string tagName, bool canAccess = true)
    {
        _viewContext = viewContext;
        _tagName = tagName;
        if (!canAccess)
        {
            _writer = viewContext.Writer;
            viewContext.Writer = TextWriter.Null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            EndContainer();
        }
    }

    public void EndContainer()
    {
        if (_writer is not null)
        {
            _viewContext.Writer = _writer;
            _writer = null;
        }
        else
        {
            _viewContext.Writer.Write("</{0}>", _tagName);
        }
    }
}
