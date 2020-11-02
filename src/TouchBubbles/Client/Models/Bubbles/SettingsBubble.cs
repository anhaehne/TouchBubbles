using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class SettingsBubble : Bubble
    {
        private readonly ISettingsService _settingsService;

        public SettingsBubble(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            BackgroundColor = "DarkGrey";
            Name = "Settings";
            Icon = "mdi-cog-outline";
            IsActive = false;
        }

        public override Task OnClickAsync()
        {
            _settingsService.ToggleSettingsEnabled();
            IsActive = !IsActive;

            return base.OnClickAsync();
        }
    }
}