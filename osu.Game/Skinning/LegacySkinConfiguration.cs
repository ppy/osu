// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;

namespace osu.Game.Skinning
{
    public class LegacySkinConfiguration : SkinConfiguration
    {
        public const decimal LATEST_VERSION = 2.7m;

        /// <summary>
        /// Legacy version of this skin.
        /// </summary>
        public decimal? LegacyVersion { get; internal set; }

        public LegacySkinConfiguration()
        {
            // Roughly matches osu!stable's slider border portions.
            // Can't use nameof(SliderBorderSize) as the lookup enum is declared in the osu! ruleset.
            ConfigDictionary["SliderBorderSize"] = 0.77f.ToString(CultureInfo.InvariantCulture);
        }

        public enum LegacySetting
        {
            Version,
            AnimationFramerate,
            LayeredHitSounds,
        }
    }
}
