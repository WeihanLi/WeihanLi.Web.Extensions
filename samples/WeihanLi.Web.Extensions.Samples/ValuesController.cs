using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Extensions.Samples
{
    [Route("/api/[controller]")]
    public class ValuesController : ControllerBase
    {
        public IActionResult Get([FromServices] IUserIdProvider userIdProvider)
        {
            // var userId = User.GetUserId<int>();

            var userId2 = userIdProvider.GetUserId();

            return Ok(new object[] { userId2 });
        }
    }
}
