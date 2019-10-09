// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacySkinConfiguration : DefaultSkinConfiguration
    {
        public const double LATEST_VERSION = 2.5;

        /// <summary>
        /// Legacy version of this skin.
        /// </summary>
        public double? LegacyVersion { get; internal set; }
    }

    public enum LegacySkinConfigurations
    {
        Version,
    }
}
