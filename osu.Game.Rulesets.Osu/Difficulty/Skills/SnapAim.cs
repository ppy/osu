// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class SnapAim : Aim
    {
        public SnapAim(Mod[] mods)
            : base(mods, false)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double snap = AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);
            double flow = FlowAimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);

            return snap * probabilityOfSnap(snap, flow);
        }

        private double probabilityOfSnap(double snap, double flow)
        {
            // If snap is easier - we always use snap
            if (snap <= flow)
                return 1.0;

            // If flow is easier - we decrease the weight of the snap difficulty accordingly
            return Math.Pow(flow / snap, 3.5);
        }
    }
}
