// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    /// <summary>
    /// A hit circle which is at the end of a slider path (either repeat or final tail).
    /// </summary>
    public abstract class SliderEndCircle : HitCircle
    {
        protected readonly Slider Slider;

        protected SliderEndCircle(Slider slider)
        {
            Slider = slider;
        }

        public int RepeatIndex { get; set; }

        public double SpanDuration => Slider.SpanDuration;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            if (RepeatIndex > 0)
            {
                // Repeat points after the first span should appear behind the still-visible one.
                TimeFadeIn = 0;

                // The next end circle should appear exactly after the previous circle (on the same end) is hit.
                TimePreempt = SpanDuration * 2;
            }
            else
            {
                // The first end circle should fade in with the slider.
                TimePreempt += StartTime - Slider.StartTime;
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override Judgement CreateJudgement() => new SliderEndJudgement();

        public class SliderEndJudgement : OsuJudgement
        {
            public override HitResult MaxResult => HitResult.LargeTickHit;
        }
    }
}
