// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public class StrainEvaluator
    {
        public static double EvaluateDifficultyOf(ManiaDifficultyHitObject current)
        {
            double sameColumn = JackEvaluator.GetDifficultyOf(current);
            double crossColumn = CrossColumnEvaluator.GetDifficultyOf(current);
            double pressingIntensity = PressingIntensityEvaluator.GetDifficultyOf(current);
            double release = ReleaseEvaluator.GetDifficultyOf(current);
            double unevenness = UnevennessEvaluator.GetValueOf(current);
            double activeKeyCount = AKCEvaluator.GetValueOf(current);
            double localNoteCount = LNCEvaluator.GetValueOf(current);

            double clampedSameColumn = Math.Min(sameColumn, 8.0 + 0.85 * sameColumn);

            // Adjust unevenness impact based on how many keys are active
            double unevennessKeyAdjustment = 1.0;
            if (unevenness > 0.0 && activeKeyCount > 0.0)
                unevennessKeyAdjustment = Math.Pow(unevenness, 3.0 / activeKeyCount);

            // Combine unevenness with same-column difficulty
            double unevennessSameColumnComponent = unevennessKeyAdjustment * clampedSameColumn;
            double firstComponent = 0.4 * Math.Pow(unevennessSameColumnComponent, 1.5);

            // Combine unevenness with pressing intensity and release difficulty
            double releaseComponent = release * DifficultyCalculationUtils.Smoothstep(activeKeyCount, 0, 4) * 35.0 / (localNoteCount + 8.0);
            double unevennessPressingReleaseComponent = Math.Pow(unevenness, 2.0 / 3.0) * (0.8 * pressingIntensity + releaseComponent);
            double secondComponent = 0.6 * Math.Pow(unevennessPressingReleaseComponent, 1.5);

            // Main strain difficulty combining both components
            double totalStrainDifficulty = Math.Pow(firstComponent + secondComponent, 2.0 / 3.0);

            // Cross-column coordination component
            double twistComponent = (unevennessKeyAdjustment * crossColumn) / (crossColumn + totalStrainDifficulty + 1.0);
            double poweredTwist = twistComponent > 0.0 ? twistComponent * Math.Sqrt(twistComponent) : 0.0;

            double finalStrain = 2.7 * Math.Sqrt(totalStrainDifficulty) * poweredTwist + totalStrainDifficulty * 0.27;

            return finalStrain;
        }
    }
}
