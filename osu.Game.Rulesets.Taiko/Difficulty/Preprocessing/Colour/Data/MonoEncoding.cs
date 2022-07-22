// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encode colour information for a sequence of <see cref="TaikoDifficultyHitObject"/>s. Consecutive <see cref="TaikoDifficultyHitObject"/>s
    /// of the same <see cref="HitType"/> are encoded within the same <see cref="MonoEncoding"/>.
    /// </summary>
    public class MonoEncoding
    {
        /// <summary>
        /// List of <see cref="DifficultyHitObject"/>s that are encoded within this <see cref="MonoEncoding"/>.
        /// This is not declared as <see cref="TaikoDifficultyHitObject"/> to avoid circular dependencies.
        /// </summary>
        public List<TaikoDifficultyHitObject> EncodedData { get; private set; } = new List<TaikoDifficultyHitObject>();

        /// <summary>
        /// The parent <see cref="ColourEncoding"/> that contains this <see cref="MonoEncoding"/>
        /// </summary>
        public ColourEncoding? Parent;

        /// <summary>
        /// Index of this encoding within it's parent encoding
        /// </summary>
        public int Index;

        /// <summary>
        /// How long the mono pattern encoded within is
        /// </summary>
        public int RunLength => EncodedData.Count;
    }
}
