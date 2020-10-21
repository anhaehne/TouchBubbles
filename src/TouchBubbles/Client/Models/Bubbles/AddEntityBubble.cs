using System.Threading.Tasks;
using TouchBubbles.Client.Components.Overlays;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class AddEntityBubble : Bubble
    {
        private readonly IOverlayService _overlayService;

        public AddEntityBubble(IOverlayService overlayService)
        {
            BackgroundColorOutline = "Gray";
            BackgroundColor = "Transparent";
            Name = "Add entity";
            Icon = "mdi-plus";
            _overlayService = overlayService;
        }

        public override Task OnClickAsync()
        {
            _overlayService.ShowOverlay<AddEntityOverlay>();
            return Task.CompletedTask;
        }
    }
}
