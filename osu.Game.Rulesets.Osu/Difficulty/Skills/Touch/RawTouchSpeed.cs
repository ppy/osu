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

        protected override double StrainValueOf(OsuDifficultyHitObject current) =>
            SpeedEvaluator.EvaluateDifficultyOf(current, false) * skillMultiplier;

        protected override double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand, TouchHand lastHand)
        {
            double singletapMultiplier = 1;

            if (currentHand == lastHand)
                // Reduction in speed value for singletapping consecutive notes.
                singletapMultiplier *= 0.93;

            return SpeedEvaluator.EvaluateDifficultyOf(simulated, true) * singletapMultiplier * skillMultiplier;
        }

        public override RawTouchSpeed DeepClone() => new RawTouchSpeed(this);
    }
}
