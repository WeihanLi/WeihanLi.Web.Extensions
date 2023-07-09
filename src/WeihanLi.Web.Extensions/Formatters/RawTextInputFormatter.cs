// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace WeihanLi.Web.Formatters;

public sealed class RawTextInputFormatter : TextInputFormatter
{
    public RawTextInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }
    
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        using var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding);
        var rawContent = await reader.ReadToEndAsync();
        return await InputFormatterResult.SuccessAsync(rawContent);
    }
}
