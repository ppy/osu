// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public static class PressingIntensityEvaluator
    {
        public static double GetDifficultyOf(ManiaDifficultyHitObject current)
        {
            var data = current.DifficultyData;
            return data.SampleFeatureAtTime(current.StartTime, data.PressingIntensity);
        }
    }
}
