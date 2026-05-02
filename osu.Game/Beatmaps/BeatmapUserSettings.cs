// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play.PlayerSettings;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// User settings overrides that are attached to a beatmap.
    /// </summary>
    public class BeatmapUserSettings : EmbeddedObject
    {
        /// <summary>
        /// An audio offset that can be used for timing adjustments.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// Determines if hitsounds for beatmap are enabled, disabled, or use the global setting.
        /// </summary>
        [Ignored]
        public HitsoundsSetting Hitsounds
        {
            get => (HitsoundsSetting)HitsoundsInt;
            set => HitsoundsInt = (int)value;
        }

        [MapTo(nameof(Hitsounds))]
        public int HitsoundsInt { get; set; } = (int)HitsoundsSetting.UseGlobalSetting;
    }
}
