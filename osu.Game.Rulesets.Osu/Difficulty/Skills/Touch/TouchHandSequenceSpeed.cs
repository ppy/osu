// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    /// <summary>
    /// Evaluates the speed difficulty of an <see cref="OsuDifficultyHitObject"/> if it was hit with a specific <see cref="TouchHand"/>.
    /// </summary>
    public class TouchHandSequenceSpeed : TouchHandSequenceSkill
    {
        private double skillMultiplier => 1375;
        protected override double StrainDecayBase => 0.3;

        public TouchHandSequenceSpeed(double clockRate)
            : base(clockRate)
        {
        }

        private TouchHandSequenceSpeed(TouchHandSequenceSpeed copy)
            : base(copy)
        {
        }

        protected override double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            double singletapMultiplier = 1.0;

            // Reduction in speed value for singletapping consecutive notes.
            if (currentHand == LastHand && currentHand != TouchHand.Drag)
                singletapMultiplier *= 0.93;

            double bonusMultiplier = 1.0;

            // Add a slight bonus for hand-coordination required to swap hands.
            if (currentHand != LastNonDragHand)
                bonusMultiplier += 0.08;

            // Add a speed bonus for swapping from dragging to tapping.
            if (currentHand == GetOtherHand(LastNonDragHand) && LastHand == TouchHand.Drag)
                bonusMultiplier += 0.2;

            // Treat drags as regular gameplay in terms of tapping.
            bool tappedWithTouch = currentHand != TouchHand.Drag;

            return SpeedEvaluator.EvaluateDifficultyOf(simulated, tappedWithTouch) * singletapMultiplier * bonusMultiplier * skillMultiplier;
        }

        public override TouchHandSequenceSpeed DeepClone() => new TouchHandSequenceSpeed(this);
    }
}
