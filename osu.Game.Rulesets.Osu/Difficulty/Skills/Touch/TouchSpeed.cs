// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public class TouchSpeed : TouchSkill
    {
        private double strainDecayBase => 0.3;

        private double currentStrain;
        private double currentRhythm;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private readonly List<double> objectStrains = new List<double>();

        private readonly double clockRate;

        public TouchSpeed(Mod[] mods, double clockRate)
            : base(mods)
        {
            this.clockRate = clockRate;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain * currentRhythm) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain = CalculateCurrentStrain(current);
            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = currentStrain * currentRhythm;

            objectStrains.Add(totalStrain);

            return totalStrain;
        }

        protected override double GetProbabilityStrain(TouchProbability probability) => probability.Skills[1].CurrentStrain;

        protected override double GetProbabilityTotalStrain(TouchProbability probability) => CalculateTotalStrain(probability.Skills[0].CurrentStrain, GetProbabilityStrain(probability));

        protected override RawTouchSkill[] GetRawSkills() => new RawTouchSkill[]
        {
            new RawTouchAim(clockRate, true),
            new RawTouchSpeed(clockRate),
        };

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
