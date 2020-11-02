using System;
using System.Threading.Tasks;

namespace TouchBubbles.Shared.Models
{
    public class Bubble
    {
        private string _backgroundColor = "Red";
        private string _icon = "mdi-progress-question";
        private bool _isActive = true;
        private string _name = "Test";
        private float _slidingValue;


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

        public float SlidingValue
        {
            get => _slidingValue;
            set
            {
                _slidingValue = value;
                OnSlidingValueChanged(value);
            }
        }

        public bool SupportsSlidingValue { get; protected set; }

        public virtual Task OnClickAsync()
        {
            return Task.CompletedTask;
        }

        public event Action? BubbleChanged;

        protected virtual void OnSlidingValueChanged(float newValue)
        {
        }

        protected void InvokeBubbleChanged()
        {
            BubbleChanged?.Invoke();
        }
    }
}