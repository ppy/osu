// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class ReadingHidden : Aim
    {
        public ReadingHidden(Mod[] mods)
            : base(mods, false)
        {
        }
        protected new double SkillMultiplier => 7.2;

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);

            // We're not using slider aim because we assuming that HD doesn't makes sliders harder (what is not true, but we will ignore this for now)
            double hiddenDifficulty = AimEvaluator.EvaluateDifficultyOf(current, false);
            hiddenDifficulty *= ReadingHiddenEvaluator.EvaluateDifficultyOf(current);
            hiddenDifficulty *= SkillMultiplier;

            CurrentStrain += hiddenDifficulty;

            return CurrentStrain;
        }

        public new static double DifficultyToPerformance(double difficulty) => Math.Max(
            Math.Max(difficulty * 16, Math.Pow(difficulty, 2) * 10), Math.Pow(difficulty, 3) * 4);
    }
}
