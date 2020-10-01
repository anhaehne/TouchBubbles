using System;
using System.Collections.Generic;
using System.Text;

namespace TouchBubbles.Shared.Models
{
    public class Bubble
    {
        public string BackgroundColor { get; set; } = "Red";

        public string BackgroundOutline { get; set; } = "Transparent";

        public string Name { get; set; } = "Test";

        public string Icon { get; set; } = "mdi-progress-question";
    }
}
