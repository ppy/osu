// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    }
}
