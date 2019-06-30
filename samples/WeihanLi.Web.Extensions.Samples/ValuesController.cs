using Microsoft.AspNetCore.Mvc;

namespace WeihanLi.Web.Extensions.Samples
{
    [Route("/api/[controller]")]
    public class ValuesController : ControllerBase
    {
        public IActionResult Get()
        {
            var userId = User.GetUserId<int>();

            return Ok(new[] { 1, 2, 3 });
        }
    }
}
