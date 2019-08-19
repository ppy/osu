// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public abstract class HitErrorDisplay : CompositeDrawable
    {
        protected readonly HitWindows HitWindows;

        public HitErrorDisplay(float overallDifficulty, HitWindows hitWindows)
        {
            HitWindows = hitWindows;
            HitWindows.SetDifficulty(overallDifficulty);
        }

        public abstract void OnNewJudgement(JudgementResult newJudgement);
    }
}
