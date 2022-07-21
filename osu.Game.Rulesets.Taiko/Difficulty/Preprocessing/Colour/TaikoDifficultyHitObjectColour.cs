// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        /// <summary>
        /// <see cref="MonoEncoding"/> encoding that encodes this note, only present if this is the first note within a
        /// <see cref="MonoEncoding"/>
        /// </summary>
        public MonoEncoding? MonoEncoding;

        /// <summary>
        /// <see cref="ColourEncoding"/> encoding that encodes this note, only present if this is the first note within
        /// a <see cref="ColourEncoding"/>
        /// </summary>
        public ColourEncoding? ColourEncoding;

        /// <summary>
        /// <see cref="CoupledColourEncoding"/> encoding that encodes this note, only present if this is the first note
        /// within a <see cref="CoupledColourEncoding"/>
        /// </summary>
        public CoupledColourEncoding? CoupledColourEncoding;
    }
}
