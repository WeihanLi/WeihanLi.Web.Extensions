// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace WeihanLi.Web.Authorization.Token;

public class TokenEntity
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}

public class TokenEntityWithRefreshToken : TokenEntity
{
    public string RefreshToken { get; set; }
}
