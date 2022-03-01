// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;

namespace WeihanLi.Web.AccessControlHelper;

public sealed class SparkContainer : IDisposable
{
    private readonly string _tagName;
    private readonly ViewContext _viewContext;
    private readonly bool _canAccess;
    private bool _disposed;
    private readonly TextWriter _writer;

    public SparkContainer(ViewContext viewContext, string tagName, bool canAccess = true)
    {
        _viewContext = viewContext;
        _tagName = tagName;
        _canAccess = canAccess;
        if (!_canAccess)
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
        if (!_canAccess)
        {
            _viewContext.Writer = _writer;
        }
        else
        {
            _viewContext.Writer.Write("</{0}>", _tagName);
        }
    }
}
