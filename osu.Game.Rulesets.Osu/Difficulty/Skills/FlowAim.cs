// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class FlowAim : Aim
    {
        public FlowAim(Mod[] mods)
            : base(mods, false)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double snap = AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);
            double flow = FlowAimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);

            return flow * probabilityOfFlow(snap, flow);
        }

        private double probabilityOfFlow(double snap, double flow)
        {
            // If flow is easier - we always use flow
            if (flow <= snap)
                return 1.0;

            // If snap is easier - we decrease the weight of the flow difficulty accordingly
            return Math.Pow(snap / flow, 3.5);
        }
    }
}
