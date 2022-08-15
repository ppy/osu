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
        /// The <see cref="MonoEncoding"/> that encodes this note, only present if this is the first note within a <see cref="MonoEncoding"/>
        /// </summary>
        public MonoEncoding? MonoEncoding;

        /// <summary>
        /// The <see cref="ColourEncoding"/> that encodes this note, only present if this is the first note within a <see cref="ColourEncoding"/>
        /// </summary>
        public ColourEncoding? ColourEncoding;

        /// <summary>
        /// The <see cref="CoupledColourEncoding"/> that encodes this note, only present if this is the first note within a <see cref="CoupledColourEncoding"/>
        /// </summary>
        public CoupledColourEncoding? CoupledColourEncoding;
    }
}
