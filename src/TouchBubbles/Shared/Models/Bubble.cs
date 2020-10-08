using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace TouchBubbles.Shared.Models
{
    public class Bubble
    {
        private string _backgroundColor = "Red";
        private string _backgroundColorOutline = "Red";
        private string _icon = "mdi-progress-question";
        private string _name = "Test";
        private float _slidingValue;
        private bool _isActive = true;


        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                InvokeBubbleChanged();
            }
        }
        public string BackgroundColorOutline
        {
            get => _backgroundColorOutline;
            set
            {
                _backgroundColorOutline = value;
                InvokeBubbleChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                InvokeBubbleChanged();
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                InvokeBubbleChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                InvokeBubbleChanged();
            }
        }

        public virtual Task OnClickAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnSlidingValueChanged(float newValue)
        {

        }

        public float SlidingValue
        {
            get => _slidingValue;
            set
            {
                _slidingValue = value;
                OnSlidingValueChanged(value);
            }
        }

        public bool SupportsSlidingValue { get; set; }

        protected void InvokeBubbleChanged() => BubbleChanged?.Invoke();

        public event Action BubbleChanged;
    }
}