using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TouchBubbles.Shared.Models
{
    public class Bubble
    {
        public virtual string BackgroundColor { get; set; } = "Red";

        public virtual string BackgroundOutline { get; set; } = "Transparent";

        public virtual string Name { get; set; } = "Test";

        public virtual string Icon { get; set; } = "mdi-progress-question";

        public virtual Task OnClickAsync() => Task.CompletedTask;
    }
}
