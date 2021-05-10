using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }

        public IActionResult GeText()
        {
            return Content("Hello World!");
        }
    }
}
