using Microsoft.AspNetCore.Mvc;
using WeihanLi.Common.Services;

namespace WeihanLi.Web.Extensions.Samples
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromServices] IUserIdProvider userIdProvider)
        {
            // var userId = User.GetUserId<int>();

            var userId2 = userIdProvider.GetUserId();

            return Ok(new object[] { userId2, new
            {
                User.Identity.IsAuthenticated,
                UserId = User.GetUserId(),
                User.Identity.Name,
                User.Identity.AuthenticationType,
            } });
        }
    }
}
