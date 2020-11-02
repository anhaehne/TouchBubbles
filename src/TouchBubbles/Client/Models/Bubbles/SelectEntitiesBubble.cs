using System.Threading.Tasks;
using TouchBubbles.Client.Components.Overlays;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class SelectEntitiesBubble : Bubble
    {
        private readonly IOverlayService _overlayService;

        public SelectEntitiesBubble(IOverlayService overlayService)
        {
            BackgroundColor = "DarkGrey";
            Name = "Select entities";
            Icon = "mdi-plus";
            _overlayService = overlayService;
        }

        public override Task OnClickAsync()
        {
            _overlayService.ShowOverlay<SelectEntitiesOverlay>();
            return Task.CompletedTask;
        }
    }
}
