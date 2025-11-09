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

            var osuCurrObj = (OsuDifficultyHitObject)current;

            double bpmBonus = 0.0;

            if (DifficultyCalculationUtils.MillisecondsToBPM(osuCurrObj.AdjustedDeltaTime) > 240)
                bpmBonus = Math.Pow((DifficultyCalculationUtils.BPMToMilliseconds(240) - osuCurrObj.AdjustedDeltaTime) / 16.5, 1.1);

            double finalValue = (1 + bpmBonus) * 1000 / osuCurrObj.AdjustedDeltaTime;

            double doubletapness = 1.0 - osuCurrObj.GetDoubletapness((OsuDifficultyHitObject?)osuCurrObj.Next(0));

            return finalValue * doubletapness;
        }
    }
}
