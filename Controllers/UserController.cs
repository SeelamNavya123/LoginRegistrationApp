using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistrationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        [Route("get-all-users")]
        public IActionResult GetUsers()
        {
            return Ok("✅ Authorized data returned!");
        }
    }
}



