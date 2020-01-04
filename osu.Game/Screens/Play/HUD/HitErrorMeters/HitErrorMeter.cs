// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public abstract class HitErrorMeter : CompositeDrawable
    {
        protected readonly HitWindows HitWindows;

        protected HitErrorMeter(HitWindows hitWindows)
        {
            HitWindows = hitWindows;
        }

        public abstract void OnNewJudgement(JudgementResult judgement);
    }
}
