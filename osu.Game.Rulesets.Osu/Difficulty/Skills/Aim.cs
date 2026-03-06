// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        public readonly bool IncludeSliders;

        public Aim(Mod[] mods, bool includeSliders)
            : base(mods)
        {
            IncludeSliders = includeSliders;
        }

        private double currentAimStrain;
        private double currentSpeedStrain;
        private double currentFlowStrain;

        private double skillMultiplierAim => 65.2;
        private double skillMultiplierSpeed => 2.7;
        private double skillMultiplierFlow => 16.5;
        private double skillMultiplierTotal => 1.0;
        private double meanExponent => 1.2;

        private readonly List<double> sliderStrains = new List<double>();

        private double strainDecayAim(double ms) => Math.Pow(0.15, ms / 1000);
        private double strainDecaySpeed(double ms) => Math.Pow(0.3, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) =>
            currentAimStrain * strainDecayAim(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double decayAim = strainDecayAim(((OsuDifficultyHitObject)current).AdjustedDeltaTime);
            double decaySpeed = strainDecaySpeed(((OsuDifficultyHitObject)current).AdjustedDeltaTime);

            double aimDifficulty = AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders) * skillMultiplierAim;
            double speedDifficulty = SpeedAimEvaluator.EvaluateDifficultyOf(current) * skillMultiplierSpeed;
            double flowDifficulty = FlowAimEvaluator.EvaluateDifficultyOf(current, IncludeSliders) * skillMultiplierFlow;

            if (Mods.Any(m => m is OsuModTouchDevice))
            {
                aimDifficulty = Math.Pow(aimDifficulty, 0.8);
                speedDifficulty = Math.Pow(speedDifficulty, 0.95);
            }

            if (Mods.Any(m => m is OsuModRelax))
            {
                speedDifficulty *= 0.0;
            }

            double totalDifficulty = calcTotalValue(aimDifficulty, speedDifficulty, flowDifficulty);

            currentAimStrain *= decayAim;
            currentAimStrain += totalDifficulty * (1 - decayAim);

            currentSpeedStrain *= decaySpeed;
            currentSpeedStrain += speedDifficulty * (1 - decaySpeed);

            currentFlowStrain *= decayAim;
            currentFlowStrain += flowDifficulty * (1 - decayAim);

            double totalStrain = currentAimStrain;//calcTotalValue(aimDifficulty, currentSpeedStrain, flowDifficulty);

            if (current.BaseObject is Slider)
                sliderStrains.Add(totalStrain);

            return totalStrain;
        }

        private double calcTotalValue(double snapDifficulty, double agilityDifficulty, double flowDifficulty)
        {
            double combinedSnapDifficulty = DifficultyCalculationUtils.Norm(meanExponent, snapDifficulty, agilityDifficulty);

            double pSnap = ProbabilityOf(flowDifficulty / combinedSnapDifficulty);
            double pFlow = 1 - pSnap;

            double totalDifficulty = combinedSnapDifficulty * pSnap + flowDifficulty * pFlow;

            double totalStrain = totalDifficulty * skillMultiplierTotal;

            return totalStrain;
        }

        private const double k = 7.27;

        // A function that turns the ratio of snap : flow into the probability of snapping/flowing
        // It has the constraints:
        // P(snap) + P(flow) = 1 (the object is always either snapped or flowed)
        // P(snap) = f(snap/flow), P(flow) = f(flow/snap) (ie snap and flow are symmetric and reversible)
        // Therefore: f(x) + f(1/x) = 1
        // 0 <= f(x) <= 1 (cannot have negative or greater than 100% probability of snapping or flowing)
        // This logistic function is a solution, which fits nicely with the general idea of interpolation and provides a tuneable constant
        protected static double ProbabilityOf(double ratio)
            => ratio == 0 ? 0 :
                double.IsNaN(ratio) ? 1 :
                (1 / (1 + Math.Exp(-k * Math.Log(ratio))));

        public double GetDifficultSliders()
        {
            if (sliderStrains.Count == 0)
                return 0;

            double maxSliderStrain = sliderStrains.Max();

            if (maxSliderStrain == 0)
                return 0;

            return sliderStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxSliderStrain * 12.0 - 6.0))));
        }

        public double CountTopWeightedSliders(double difficultyValue)
            => OsuStrainUtils.CountTopWeightedSliders(sliderStrains, difficultyValue);
    }
}
