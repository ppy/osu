// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : OsuStrainSkill
    {
        private readonly double ClockRate;
        private readonly bool hasHiddenMod;
        private readonly double Preempt;
        private double skillMultiplier => 9.0;
        private double currentStrain;
        private double strainDecayBase => 0.3;

        public Reading(IBeatmap beatmap, Mod[] mods, double clockRate)
            : base(mods)
        {
            ClockRate = clockRate;
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
            Preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);
        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += ReadingEvaluator.EvaluateDifficultyOf(current, ClockRate, Preempt, hasHiddenMod) * skillMultiplier;

            return currentStrain;
        }

        public new static double DifficultyToPerformance(double difficulty) => 25 * Math.Pow(difficulty, 2);
    }
}
