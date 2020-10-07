using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TouchBubbles.Server.Services;

namespace TouchBubbles.Server.Controllers
{
    [Route("homeassistant")]
    public class HomeAssistantController : Controller
    {
        private readonly IHomeAssistantService _homeAssistantService;

        public HomeAssistantController(IHomeAssistantService homeAssistantService)
        {
            _homeAssistantService = homeAssistantService;
        }

        [HttpGet("states")]
        public async Task GetStatesAsync()
        {
            Response.StatusCode = 200;
            Response.ContentType = "application/json";
            await _homeAssistantService.WriteStatesResponseToStream(Response.Body).ConfigureAwait(false);
        }


        [HttpPost("services/{domain}/{service}")]
        public async Task CallService(string domain, string service)
        {
            Response.StatusCode = 200;
            Response.ContentType = "application/json";
            await _homeAssistantService.CallServiceAsync(domain, service, Request.Body, Response.Body).ConfigureAwait(false);
        }
    }
}