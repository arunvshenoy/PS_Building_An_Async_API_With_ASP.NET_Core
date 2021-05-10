using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookCovers.API.Controllers
{
    [Route("api/bookcovers")]
    [ApiController]
    public class BookCoversController : ControllerBase
    {
        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetBookCover(string name, bool returnFault = false)
        {
            //If returnFault is true, wiat 500ms and return an Internal Server Error
            if (returnFault)
            {
                await Task.Delay(500);
                return new StatusCodeResult(500);
            }

            //Generate a book cover (byte array) between 2 and 100 MB
            var random = new Random();
            var fakeCoverBytes = random.Next(2097152, 10485760);
            var fakeCover = new byte[fakeCoverBytes];
            random.NextBytes(fakeCover);

            //await Task.Delay(200);

            return Ok(new
            {
                Name = name,
                Content = fakeCover
            });
        }
    }
}
