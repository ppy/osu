// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    /// <summary>
    /// Evaluates the aim difficulty of an <see cref="OsuDifficultyHitObject"/> if it was hit with a specific <see cref="TouchHand"/>.
    /// </summary>
    public class TouchHandSequenceAim : TouchHandSequenceSkill
    {
        private readonly bool withSliders;

        private double skillMultiplier => 23.55;

        public TouchHandSequenceAim(double clockRate, bool withSliders)
            : base(clockRate)
        {
            this.withSliders = withSliders;
        }

        private TouchHandSequenceAim(TouchHandSequenceAim copy)
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
                bonusMultiplier += 0.25;

                // Add an obstrution bonus if the most recent instance of the "other hand" is in between the current object and the previous object with the actual hand.
                double simulatedAngle = getSwapAngle((OsuHitObject)simulated.BaseObject, currentHand);

                bonusMultiplier += 0.5 / (1 + Math.Exp(-(simulatedAngle - 3 * Math.PI / 5) / 9));
            }

            // Add a slight aim bonus for swapping to dragging after tapping.
            if (currentHand != LastHand && currentHand == TouchHand.Drag)
                bonusMultiplier += 0.08;

            double aimValue = AimEvaluator.EvaluateDifficultyOf(simulated, withSliders);
            double aimValueNoSliders = AimEvaluator.EvaluateDifficultyOf(simulated, false);

            // Only apply co-ordination bonuses to regular aim and not slider aim.
            return (aimValueNoSliders * bonusMultiplier + (aimValue - aimValueNoSliders)) * skillMultiplier;
        }

        /// <summary>
        /// Calculates the angle of an <see cref="OsuHitObject"/> relative to the last <see cref="OsuHitObject"/>s with different <see cref="TouchHand"/>s.
        /// </summary>
        /// <remarks>
        /// The angle is formed by <paramref name="current"/>, the last <see cref="OsuHitObject"/> hit by the <see cref="TouchHand"/>
        /// that is the opposite of <paramref name="hand"/>, and the last <see cref="OsuHitObject"/> hit by <paramref name="hand"/>.
        /// </remarks>
        /// <param name="current">The <see cref="OsuHitObject"/> for which the angle is to be calculated.</param>
        /// <param name="hand">The <see cref="TouchHand"/> that hit the <see cref="OsuHitObject"/>.</param>
        /// <returns>The angle.</returns>
        private double getSwapAngle(OsuHitObject current, TouchHand hand)
        {
            var otherHand = GetOtherHand(hand);

            var last = GetLastObjects(otherHand).Last();
            var lastLast = GetLastObjects(hand).Last();

            Vector2 currentPos = current.Position;
            Vector2 lastPos = (last as Slider)?.LazyEndPosition ?? last.Position;
            Vector2 lastLastPos = (lastLast as Slider)?.LazyEndPosition ?? lastLast.Position;

            Vector2 vectorA = lastPos - lastLastPos;
            Vector2 vectorB = currentPos - lastPos;

            float dot = Vector2.Dot(vectorA, vectorB);
            float det = vectorA.X * vectorB.Y - vectorA.Y * vectorB.X;

            return Math.Abs(Math.Atan2(det, dot));
        }

        public override TouchHandSequenceAim DeepClone() => new TouchHandSequenceAim(this);
    }
}
