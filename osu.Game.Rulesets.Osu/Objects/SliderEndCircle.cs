﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    /// <summary>
    /// A hit circle which is at the end of a slider path (either repeat or final tail).
    /// </summary>
    public abstract class SliderEndCircle : HitCircle
    {
        private readonly Slider slider;

        protected SliderEndCircle(Slider slider)
        {
            this.slider = slider;
        }

        public int RepeatIndex { get; set; }

        public double SpanDuration => slider.SpanDuration;

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
                TimePreempt += StartTime - slider.StartTime;
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
