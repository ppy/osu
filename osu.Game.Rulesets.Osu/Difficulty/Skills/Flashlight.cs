// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to memorise and hit every object in a map with the Flashlight mod enabled.
    /// </summary>
    public class Flashlight : StrainSkill
    {
        protected override double SkillMultiplier => 0.05512;
        protected override double StrainDecayBase => 0.15;
        protected override double SumDecayExponent => 0.9;

        public Flashlight(Mod[] mods)
            : base(mods)
        {
        }

        protected override double ObjectDifficultyOf(DifficultyHitObject current) => FlashlightEvaluator.EvaluateDifficultyOf(current, Mods);

        public override double DifficultyValue() => GetCurrentStrainPeaks().Sum();

        public static double DifficultyToPerformance(double difficulty) => 25 * Math.Pow(difficulty, 2);
    }
}
