// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public abstract class HitErrorMeter : CompositeDrawable
    {
        protected readonly HitWindows HitWindows;

        [Resolved]
        private OsuColour colours { get; set; }

        protected HitErrorMeter(HitWindows hitWindows)
        {
            HitWindows = hitWindows;
        }

        public abstract void OnNewJudgement(JudgementResult judgement);

        protected Color4 GetColourForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return colours.Red;

                case HitResult.Meh:
                    return colours.Yellow;

                case HitResult.Ok:
                    return colours.Green;

                case HitResult.Good:
                    return colours.GreenLight;

                case HitResult.Great:
                    return colours.Blue;

                default:
                    return colours.BlueLight;
            }
        }
    }
}
