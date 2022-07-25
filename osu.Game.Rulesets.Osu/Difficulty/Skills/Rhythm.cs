// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Rhythm : OsuStrainSkill
    {
        private double currentStrain;
        private readonly double greatWindow;
        private double strainDecayBase => 0.3;

        protected override double StarsPerDouble => 1.75;
        protected override int ReducedSectionCount => 5;

        public Rhythm(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            greatWindow = hitWindowGreat;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double rhythmMultiplier = RhythmEvaluator.EvaluateDifficultyOf(current, greatWindow);

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += rhythmMultiplier - 1;

            return currentStrain;
        }
    }
}