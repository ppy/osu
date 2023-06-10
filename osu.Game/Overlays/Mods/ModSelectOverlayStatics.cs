// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// Stores global mod overlay statics. These will not be stored after disposal of <see cref="ModSelectOverlay"/>
    /// </summary>
    public class ModSelectOverlayStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults()
        {
            SetDefault(Static.LastModSelectPanelSamplePlaybackTime, (double?)null);
        }
    }

    public enum Static
    {
        /// <summary>
        /// The last playback time in milliseconds of an on/off sample (from <see cref="ModSelectPanel"/>).
        /// Used to debounce <see cref="ModSelectPanel"/> on/off sounds game-wide to avoid volume saturation, especially in activating mod presets with many mods.
        /// </summary>
        LastModSelectPanelSamplePlaybackTime
    }
}
