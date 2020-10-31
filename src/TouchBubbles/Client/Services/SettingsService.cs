using System;

namespace TouchBubbles.Client.Services
{
    public interface ISettingsService
    {
        bool AreBubbleControlsEnabled { get; set; }
        event EventHandler<bool>? BubbleControlsEnabledChanged;
    }

    public class SettingsService : ISettingsService
    {
        private bool _areBubbleControlsEnabled;

        public event EventHandler<bool>? BubbleControlsEnabledChanged;

        public bool AreBubbleControlsEnabled
        {
            get => _areBubbleControlsEnabled;
            set
            {
                _areBubbleControlsEnabled = value;
                BubbleControlsEnabledChanged?.Invoke(this, value);
            }
        }
    }
}