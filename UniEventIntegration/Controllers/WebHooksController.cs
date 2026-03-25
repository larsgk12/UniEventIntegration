using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UniEventIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebHooksController : ControllerBase
    {

        public WebHooksController() { }

        [HttpGet(Name = "Get")]
        public string Get()
        {
            return "Hello from UniEventIntegration WebHooksController!";
        }


    }
}
