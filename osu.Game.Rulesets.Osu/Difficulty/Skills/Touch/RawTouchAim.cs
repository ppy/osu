// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public class RawTouchAim : RawTouchSkill
    {
        private readonly bool withSliders;

        private double skillMultiplier => 23.55;

        public RawTouchAim(double clockRate, bool withSliders)
            : base(clockRate)
        {
            this.withSliders = withSliders;
        }

        private RawTouchAim(RawTouchAim copy)
            : base(copy)
        {
            withSliders = copy.withSliders;
        }

        protected override double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            double bonusMultiplier = 1.0;

            // Add a bonus for the hand co-ordination required to aim with both hands.
            if (currentHand != LastNonDragHand && currentHand != TouchHand.Drag)
            {
                bonusMultiplier += 0.3;

                // Add an obstrution bonus if the most recent instance of the "other hand" is in between the current object and the previous object with the actual hand.
                var simulatedSwap = CreateSimulatedSwapObject(simulated, currentHand);

                if (simulatedSwap.Angle != null)
                    bonusMultiplier += 0.9 / (1 + Math.Exp(-(simulatedSwap.Angle.Value - 3 * Math.PI / 5) / 9));
            }

            // Add a slight aim bonus for swapping to dragging after tapping.
            if (currentHand != LastHand && currentHand == TouchHand.Drag)
                bonusMultiplier += 0.125;

            // Decay the bonuses by strain time.
            bonusMultiplier /= 1 + simulated.StrainTime / 1000;

            return AimEvaluator.EvaluateDifficultyOf(simulated, withSliders) * bonusMultiplier * skillMultiplier;
        }

        public override RawTouchAim DeepClone() => new RawTouchAim(this);
    }
}
