using System.Threading.Tasks;
using Microsoft.JSInterop;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class ReloadBubble : Bubble
    {
        private readonly IJSRuntime _jsRuntime;

        public ReloadBubble(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            BackgroundColor = "DarkGray";
            Name = "Reload page";
            Icon = "mdi-reload";
        }

        public override async Task OnClickAsync()
        {
            await _jsRuntime.InvokeVoidAsync("location.reload");
        }
    }
}