using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchBubbles.Client.Components.Overlays;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class SelectProfileBubble : Bubble
    {
        private readonly IOverlayService _overlayService;

        public SelectProfileBubble(IOverlayService overlayService)
        {
            BackgroundColorOutline = "Gray";
            BackgroundColor = "Transparent";
            Name = "Select profile";
            Icon = "mdi-arrow-left-right";
            _overlayService = overlayService;
        }

        public override Task OnClickAsync()
        {
            _overlayService.ShowOverlay<SelectProfileOverlay>();
            return Task.CompletedTask;
        }
    }
}
