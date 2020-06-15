// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuScoreProcessor : ScoreProcessor
    {
        /// <summary>
        /// The number of bins on each side of the timing distribution.
        /// </summary>
        private const int timing_distribution_bins = 25;

        /// <summary>
        /// The total number of bins in the timing distribution, including bins on both sides and the centre bin at 0.
        /// </summary>
        private const int total_timing_distribution_bins = timing_distribution_bins * 2 + 1;

        /// <summary>
        /// The centre bin, with a timing distribution very close to/at 0.
        /// </summary>
        private const int timing_distribution_centre_bin_index = timing_distribution_bins;

        private TimingDistribution timingDistribution;
        private readonly List<HitOffset> hitOffsets = new List<HitOffset>();

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            var hitWindows = CreateHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            timingDistribution = new TimingDistribution(total_timing_distribution_bins, hitWindows.WindowFor(hitWindows.LowestSuccessfulHitResult()) / timing_distribution_bins);

            base.ApplyBeatmap(beatmap);
        }

        private OsuHitCircleJudgementResult lastCircleResult;

        protected override void OnResultApplied(JudgementResult result)
        {
            base.OnResultApplied(result);

            if (result.IsHit)
            {
                int binOffset = (int)(result.TimeOffset / timingDistribution.BinSize);
                timingDistribution.Bins[timing_distribution_centre_bin_index + binOffset]++;

                addHitOffset(result);
            }
        }

        protected override void OnResultReverted(JudgementResult result)
        {
            base.OnResultReverted(result);

            if (result.IsHit)
            {
                int binOffset = (int)(result.TimeOffset / timingDistribution.BinSize);
                timingDistribution.Bins[timing_distribution_centre_bin_index + binOffset]--;

                removeHitOffset(result);
            }
        }

        private void addHitOffset(JudgementResult result)
        {
            if (!(result is OsuHitCircleJudgementResult circleResult))
                return;

            if (lastCircleResult == null)
            {
                lastCircleResult = circleResult;
                return;
            }

            if (circleResult.HitPosition != null)
            {
                Debug.Assert(circleResult.Radius != null);
                hitOffsets.Add(new HitOffset(lastCircleResult.HitCircle.StackedEndPosition, circleResult.HitCircle.StackedEndPosition, circleResult.HitPosition.Value, circleResult.Radius.Value));
            }

            lastCircleResult = circleResult;
        }

        private void removeHitOffset(JudgementResult result)
        {
            if (!(result is OsuHitCircleJudgementResult circleResult))
                return;

            if (hitOffsets.Count > 0 && circleResult.HitPosition != null)
                hitOffsets.RemoveAt(hitOffsets.Count - 1);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            timingDistribution.Bins.AsSpan().Clear();
            hitOffsets.Clear();
        }

        public override void PopulateScore(ScoreInfo score)
        {
            base.PopulateScore(score);

            score.ExtraStatistics["timing_distribution"] = timingDistribution;
            score.ExtraStatistics["hit_offsets"] = hitOffsets;
        }

        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement)
        {
            switch (hitObject)
            {
                case HitCircle _:
                    return new OsuHitCircleJudgementResult(hitObject, judgement);

                default:
                    return new OsuJudgementResult(hitObject, judgement);
            }
        }

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
