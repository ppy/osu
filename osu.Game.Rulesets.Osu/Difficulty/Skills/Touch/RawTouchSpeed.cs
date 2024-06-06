// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public class RawTouchSpeed : RawTouchSkill
    {
        private double skillMultiplier => 1375;
        protected override double StrainDecayBase => 0.3;

        public RawTouchSpeed(double clockRate)
            : base(clockRate)
        {
        }

        private RawTouchSpeed(RawTouchSpeed copy)
            : base(copy)
        {
        }

        protected override double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            double singletapMultiplier = 1;

            if (currentHand == LastHand && currentHand != TouchHand.Drag)
            {
                // Reduction in speed value for singletapping consecutive notes.
                singletapMultiplier *= 0.93;
            }

            // Treat drags as regular gameplay in terms of tapping.
            bool tappedWithTouch = currentHand != TouchHand.Drag;

            double bonusMultiplier = 1.0;
            // Add a speed bonus for swapping out from dragging back to tapping.
            if (currentHand != LastHand && LastHand == TouchHand.Drag)
                bonusMultiplier += 1.0;

            // Add a slight bonus for switching from tapping to dragging.
            if (currentHand != LastHand && currentHand == TouchHand.Drag)
                bonusMultiplier += 0.3;

            return SpeedEvaluator.EvaluateDifficultyOf(simulated, tappedWithTouch) * singletapMultiplier * bonusMultiplier * skillMultiplier;
        }

        public override RawTouchSpeed DeepClone() => new RawTouchSpeed(this);
    }
}
