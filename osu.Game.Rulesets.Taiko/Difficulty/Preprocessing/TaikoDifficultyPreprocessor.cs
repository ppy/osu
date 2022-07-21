// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyPreprocessor
    {
        /// <summary>
        /// Does preprocessing on a list of <see cref="TaikoDifficultyHitObject"/>s.
        /// TODO: Review this - this is currently only a one-step process, but will potentially be expanded in the future.
        /// </summary>
        public static List<DifficultyHitObject> Process(List<DifficultyHitObject> difficultyHitObjects)
        {
            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            return difficultyHitObjects;
        }
    }
}
