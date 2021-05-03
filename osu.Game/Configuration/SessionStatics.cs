﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Configuration
{
    /// <summary>
    /// Stores global per-session statics. These will not be stored after exiting the game.
    /// </summary>
    public class SessionStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults() => ResetValues();

        public void ResetValues()
        {
            ensureDefault(SetDefault(Static.LoginOverlayDisplayed, false));
            ensureDefault(SetDefault(Static.MutedAudioNotificationShownOnce, false));
            ensureDefault(SetDefault(Static.LowBatteryNotificationShownOnce, false));
            ensureDefault(SetDefault(Static.LastHoverSoundPlaybackTime, (double?)null));
            ensureDefault(SetDefault<APISeasonalBackgrounds>(Static.SeasonalBackgrounds, null));
        }

        private void ensureDefault<T>(Bindable<T> bindable) => bindable.SetDefault();
    }

    public enum Static
    {
        LoginOverlayDisplayed,
        MutedAudioNotificationShownOnce,
        LowBatteryNotificationShownOnce,

        /// <summary>
        /// Info about seasonal backgrounds available fetched from API - see <see cref="APISeasonalBackgrounds"/>.
        /// Value under this lookup can be <c>null</c> if there are no backgrounds available (or API is not reachable).
        /// </summary>
        SeasonalBackgrounds,

        /// <summary>
        /// The last playback time in milliseconds of a hover sample (from <see cref="HoverSounds"/>).
        /// Used to debounce hover sounds game-wide to avoid volume saturation, especially in scrolling views with many UI controls like <see cref="SettingsOverlay"/>.
        /// </summary>
        LastHoverSoundPlaybackTime
    }
}
