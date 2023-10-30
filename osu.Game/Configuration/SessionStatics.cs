// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;

namespace osu.Game.Configuration
{
    /// <summary>
    /// Stores global per-session statics. These will not be stored after exiting the game.
    /// </summary>
    public class SessionStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults()
        {
            SetDefault(Static.LoginOverlayDisplayed, false);
            SetDefault(Static.MutedAudioNotificationShownOnce, false);
            SetDefault(Static.LowBatteryNotificationShownOnce, false);
            SetDefault(Static.FeaturedArtistDisclaimerShownOnce, false);
            SetDefault(Static.LastHoverSoundPlaybackTime, (double?)null);
            SetDefault(Static.LastModSelectPanelSamplePlaybackTime, (double?)null);
            SetDefault<APISeasonalBackgrounds>(Static.SeasonalBackgrounds, null);
            SetDefault(Static.TouchInputActive, false);
        }

        /// <summary>
        /// Revert statics to their defaults after being idle for appropriate amount of time.
        /// </summary>
        /// <remarks>
        /// This only affects a subset of statics which the user would expect to have reset after a break.
        /// </remarks>
        public void ResetAfterInactivity()
        {
            GetBindable<bool>(Static.LoginOverlayDisplayed).SetDefault();
            GetBindable<bool>(Static.MutedAudioNotificationShownOnce).SetDefault();
            GetBindable<bool>(Static.LowBatteryNotificationShownOnce).SetDefault();
        }
    }

    public enum Static
    {
        LoginOverlayDisplayed,
        MutedAudioNotificationShownOnce,
        LowBatteryNotificationShownOnce,
        FeaturedArtistDisclaimerShownOnce,

        /// <summary>
        /// Info about seasonal backgrounds available fetched from API - see <see cref="APISeasonalBackgrounds"/>.
        /// Value under this lookup can be <c>null</c> if there are no backgrounds available (or API is not reachable).
        /// </summary>
        SeasonalBackgrounds,

        /// <summary>
        /// The last playback time in milliseconds of a hover sample (from <see cref="HoverSounds"/>).
        /// Used to debounce hover sounds game-wide to avoid volume saturation, especially in scrolling views with many UI controls like <see cref="SettingsOverlay"/>.
        /// </summary>
        LastHoverSoundPlaybackTime,

        /// <summary>
        /// The last playback time in milliseconds of an on/off sample (from <see cref="ModSelectPanel"/>).
        /// Used to debounce <see cref="ModSelectPanel"/> on/off sounds game-wide to avoid volume saturation, especially in activating mod presets with many mods.
        /// </summary>
        LastModSelectPanelSamplePlaybackTime,

        /// <summary>
        /// Whether the last positional input received was a touch input.
        /// Used in touchscreen detection scenarios (<see cref="TouchInputInterceptor"/>).
        /// </summary>
        TouchInputActive,
    }
}
