using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace TouchBubbles.Shared.Models
{
    public class Bubble
    {
        private string _backgroundColor = "Red";
        private string _icon = "mdi-progress-question";
        private string _name = "Test";
        private float _slidingValue;

        private readonly object _lock = new object();
        private readonly Timer _timer;
        private bool _isActive = true;

        public Bubble()
        {
            _timer = new Timer(200) { AutoReset = false };
            _timer.Elapsed += (sender, args) => OnSlidingValueChanged(_slidingValue);
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
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
                if (_slidingValue == value)
                    return;

                _slidingValue = value;

                // Reset time to debounce the OnSlidingValueChanged callback
                lock (_lock)
                {
                    _timer.Stop();
                    _timer.Start();
                }
            }
        }

        public bool SupportsSlidingValue { get; set; }

        protected void InvokeBubbleChanged() => BubbleChanged?.Invoke();

        public event Action BubbleChanged;
    }
}