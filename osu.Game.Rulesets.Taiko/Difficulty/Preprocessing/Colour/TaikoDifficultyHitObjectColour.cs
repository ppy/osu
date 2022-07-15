// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>. This is only present for the
    /// first <see cref="TaikoDifficultyHitObject"/> in a <see cref="CoupledColourEncoding"/> chunk.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        public CoupledColourEncoding Encoding { get; }

        public double EvaluatedDifficulty = 0;

        public TaikoDifficultyHitObjectColour(CoupledColourEncoding encoding)
        {
            Encoding = encoding;
        }
    }
}
