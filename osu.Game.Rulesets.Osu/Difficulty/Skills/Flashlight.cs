// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to memorise and hit every object in a map with the Flashlight mod enabled.
    /// </summary>
    public class Flashlight : StrainSkill
    {
        private readonly bool hasHiddenMod;

        public Flashlight(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);
            CurrentStrain += FlashlightEvaluator.EvaluateDifficultyOf(current, hasHiddenMod);

            return CurrentStrain;
        }

        public override double DifficultyValue() => Math.Sqrt(GetCurrentStrainPeaks().Sum()) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;

        public static double DifficultyToPerformance(double difficulty) => 28.5 * Math.Pow(difficulty, 2);
    }
}
