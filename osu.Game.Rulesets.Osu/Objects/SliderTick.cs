// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderTick : OsuHitObject
    {
        public int SpanIndex { get; set; }
        public double SpanStartTime { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            double offset;

            if (SpanIndex > 0)
                // Adding 200 to include the offset stable used.
                // This is so on repeats ticks don't appear too late to be visually processed by the player.
                offset = 200;
            else
                offset = TimeFadeIn * 0.66f;

            TimePreempt = (StartTime - SpanStartTime) / 2 + offset;
        }

        public override Judgement CreateJudgement() => new OsuJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
