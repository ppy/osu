// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            get => Enum.TryParse(HitsoundsStateString, out HitsoundsSetting hitsoundsState)
                ? hitsoundsState
                : HitsoundsSetting.UseGlobalSetting;
            set => HitsoundsStateString = value.ToString();
        }

        [MapTo(nameof(Hitsounds))]
        public string HitsoundsStateString { get; set; } = HitsoundsSetting.UseGlobalSetting.ToString();

    }
}
