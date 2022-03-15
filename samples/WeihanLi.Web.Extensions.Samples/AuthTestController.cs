// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using WeihanLi.Web.Extensions.Samples.Filters;

namespace WeihanLi.Web.Extensions.Samples;

[TestAuthFilter(Role = "Admin")]
[Route("api/authTest")]
public class AuthTestController : ControllerBase
{
    [TestAuthFilter(Role = "User")]
    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}
