using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Microsoft.JSInterop;
using TouchBubbles.Client.Models.Bubbles;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client.Services
{
    public interface ISettingsService
    {
        IObservable<bool> AreSettingEnabled { get; }

        IObservable<IReadOnlyCollection<Bubble>> DefaultBubbles { get; }

        void ToggleSettingsEnabled();
    }

    public class SettingsService : ISettingsService
    {
        private readonly BehaviorSubject<IReadOnlyCollection<Bubble>> _defaultBubbles;
        private readonly BehaviorSubject<bool> _areSettingEnabled;
        private readonly IReadOnlyCollection<Bubble> _settingsOffDefaultBubbles;
        private readonly IReadOnlyCollection<Bubble> _settingsOnDefaultBubbles;

        public SettingsService(IOverlayService overlayService, IJSRuntime jsRuntime)
        {
            var settingsBubble = new SettingsBubble(this);
            _settingsOffDefaultBubbles = new Bubble[] { settingsBubble };
            _settingsOnDefaultBubbles = new Bubble[]
            {
                new SelectEntitiesBubble(overlayService),
                new ReloadBubble(jsRuntime),
                new SelectProfileBubble(overlayService),
                settingsBubble
            };

            _defaultBubbles = new BehaviorSubject<IReadOnlyCollection<Bubble>>(_settingsOffDefaultBubbles);
            _areSettingEnabled = new BehaviorSubject<bool>(false);
        }

        public IObservable<bool> AreSettingEnabled => _areSettingEnabled;

        public IObservable<IReadOnlyCollection<Bubble>> DefaultBubbles => _defaultBubbles;

        public void ToggleSettingsEnabled()
        {
            if (_areSettingEnabled.Value)
            {
                _areSettingEnabled.OnNext(false);
                _defaultBubbles.OnNext(_settingsOffDefaultBubbles);
            }
            else
            {
                _areSettingEnabled.OnNext(true);
                _defaultBubbles.OnNext(_settingsOnDefaultBubbles);
            }
        }
    }
}