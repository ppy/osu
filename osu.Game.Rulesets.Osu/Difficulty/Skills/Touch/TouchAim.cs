// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public class TouchAim : TouchSkill
    {
        private double strainDecayBase => 0.15;

        private readonly double clockRate;
        private readonly bool withSliders;

        private double currentStrain;

        public TouchAim(Mod[] mods, double clockRate, bool withSliders)
            : base(mods)
        {
            this.clockRate = clockRate;
            this.withSliders = withSliders;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain = CalculateCurrentStrain(current);

            return currentStrain;
        }

        protected override double GetProbabilityStrain(TouchProbability probability) => probability.Skills[0].CurrentStrain;

        protected override double GetProbabilityTotalStrain(TouchProbability probability) => CalculateTotalStrain(GetProbabilityStrain(probability), probability.Skills[1].CurrentStrain);

        protected override RawTouchSkill[] GetRawSkills() => new RawTouchSkill[]
        {
            new RawTouchAim(clockRate, withSliders),
            new RawTouchSpeed(clockRate),
        };
    }
}
