using System;
using Microsoft.AspNetCore.Components;

namespace TouchBubbles.Client.Services
{
    public interface IOverlayService
    {
        void ShowOverlay<TComponent>(string backgroundColor = "gray")
            where TComponent : IComponent;

        event EventHandler<OverlayOptions?> OverlayShowing;

        void HideOverlay();
    }

    public class OverlayService : IOverlayService
    {
        public void ShowOverlay<TComponent>(string backgroundColor = "gray")
            where TComponent : IComponent
        {
            OverlayShowing?.Invoke(
                this,
                new OverlayOptions (typeof(TComponent), backgroundColor));
        }


        public event EventHandler<OverlayOptions?>? OverlayShowing;

        public void HideOverlay()
        {
            OverlayShowing?.Invoke(this, null);
        }
    }

    public class OverlayOptions
    {
        public OverlayOptions(Type contentType, string backgroundColor)
        {
            ContentType = contentType;
            BackgroundColor = backgroundColor;
        }

        public Type ContentType { get; }

        public string BackgroundColor { get; }
    }
}