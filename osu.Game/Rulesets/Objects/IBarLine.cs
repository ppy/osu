// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Interface for bar line hitobjects.
    /// Used to decouple bar line generation from ruleset-specific rendering/drawing hierarchies.
    /// </summary>
    public interface IBarLine
    {
        /// <summary>
        /// The time position of the bar.
        /// </summary>
        double StartTime { set; }

        /// <summary>
        /// Whether this bar line is a prominent beat (based on time signature of beatmap).
        /// </summary>
        bool Major { set; }
    }
}
