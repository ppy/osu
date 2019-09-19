// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// A hit object representing the end of a bar.
    /// </summary>
    public class BarLine : HitObject
    {
        /// <summary>
        /// Whether this barline is a prominent beat (based on time signature of beatmap).
        /// </summary>
        public bool Major;
    }
}
