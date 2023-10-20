// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Tap : OsuStrainSkill
    {
        private double skillMultiplier => 1375;
        private double strainDecayBase => 0.3;

        private double currentStrain;
        private double currentRhythm;

        protected override int ReducedSectionCount => 5;
        protected override double StarsPerDouble => 1.1;
        private readonly double greatWindow;

        private readonly List<double> objectStrains = new List<double>();

        public Tap(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            greatWindow = hitWindowGreat;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain * currentRhythm) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrain += TapEvaluator.EvaluateDifficultyOf(current, greatWindow) * skillMultiplier;

            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current, greatWindow);

            double totalStrain = currentStrain * currentRhythm;

            objectStrains.Add(totalStrain);

            return totalStrain;
        }

        public double RelevantNoteCount()
        {
            if (objectStrains.Count == 0)
                return 0;

            double maxStrain = objectStrains.Max();

            if (maxStrain == 0)
                return 0;

            return objectStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxStrain * 12.0 - 6.0))));
        }
    }
}
