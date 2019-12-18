﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    internal class OsuScoreProcessor : ScoreProcessor
    {
        public OsuScoreProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        private float hpDrainRate;

        protected override void ApplyBeatmap(IBeatmap beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;
        }

        protected override double HealthAdjustmentFactorFor(JudgementResult result)
            => result.Type switch
            {
                HitResult.Great => 10.2 - hpDrainRate,
                HitResult.Good => 8 - hpDrainRate,
                HitResult.Meh => 4 - hpDrainRate,
                //HitResult.SliderTick => Math.Max(7 - hpDrainRate, 0) * 0.01;
                HitResult.Miss => hpDrainRate,
                _ => 0,
            };

        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new OsuJudgementResult(hitObject, judgement);

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
