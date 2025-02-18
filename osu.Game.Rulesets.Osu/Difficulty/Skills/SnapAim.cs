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
        public SnapAim(Mod[] mods) : base(mods, true)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double snap = AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);
            double flow = FlowAimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);

            if (flow < snap)
                return flow * Math.Max(Math.Pow(flow / snap, 1.5), 0.1);

            return snap;
        }
    }
}
