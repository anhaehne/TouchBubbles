using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TouchBubbles.Shared;

namespace TouchBubbles.Server.Services
{
    public interface IHomeAssistantService
    {
        /// <summary>
        /// Requests the current states from home assistant and writes the json response to the provided stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        Task WriteStatesResponseToStream(Stream stream);

        Task CallServiceAsync(string domain, string service, Stream contentBody, Stream responseStream);
    }

    public class HomeAssistantService : IHomeAssistantService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeAssistantService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task WriteStatesResponseToStream(Stream stream)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.HomeAssistant);

            await using var responseStream = await haClient.GetStreamAsync("api/states");
            await responseStream.CopyToAsync(stream);
        }

        public async Task CallServiceAsync(string domain, string service, Stream contentBody, Stream responseStream)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.HomeAssistant);
            var response = await haClient.PostAsync($"api/services/{domain}/{service}", new StreamContent(contentBody));
            await response.Content.CopyToAsync(responseStream);
        }
    }
}