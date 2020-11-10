// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacySkinConfiguration : SkinConfiguration
    {
        public const decimal LATEST_VERSION = 2.7m;

        /// <summary>
        /// Legacy version of this skin.
        /// </summary>
        public decimal? LegacyVersion { get; internal set; }

        public enum LegacySetting
        {
            Version,
            ComboPrefix,
            ComboOverlap,
            ScorePrefix,
            ScoreOverlap,
            AnimationFramerate,
            LayeredHitSounds
        }
    }
}
