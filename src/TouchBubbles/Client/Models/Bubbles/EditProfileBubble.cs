using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class EditProfileBubble : Bubble
    {
        private readonly ISettingsService _settingsService;

        public EditProfileBubble(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            BackgroundColor = "LightGray";
            BackgroundColorOutline = "Gray";
            Name = "Edit";
            Icon = "mdi-pencil";
        }

        public override Task OnClickAsync()
        {
            _settingsService.AreBubbleControlsEnabled = !_settingsService.AreBubbleControlsEnabled;
            return base.OnClickAsync();
        }
    }
}
