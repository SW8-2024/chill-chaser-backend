using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChillChaser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCollectionController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public IActionResult Notification()
        {
            
        }
    }
}
