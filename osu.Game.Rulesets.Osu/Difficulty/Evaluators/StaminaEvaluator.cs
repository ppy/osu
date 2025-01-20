// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class StaminaEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            // derive strainTime for calculation
            var osuCurrObj = (OsuDifficultyHitObject)current;

            double strainTime = osuCurrObj.StrainTime;
            double speedBonus = 0.0;

            if (DifficultyCalculationUtils.MillisecondsToBPM(strainTime) > 240)
                speedBonus = Math.Pow((DifficultyCalculationUtils.BPMToMilliseconds(240) - strainTime) / 17.0, 1.5);

            double finalValue = (1 + speedBonus) * 1000 / strainTime;

            double lowBpmPenalty = Math.Clamp(1 + (DifficultyCalculationUtils.BPMToMilliseconds(220) - strainTime) / 95, 0.0, 1.0);
            finalValue *= lowBpmPenalty;

            return finalValue;
        }
    }
}
