// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
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

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            var hitWindows = CreateHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            timingDistribution = new TimingDistribution(total_timing_distribution_bins, hitWindows.WindowFor(hitWindows.LowestSuccessfulHitResult()) / timing_distribution_bins);

            base.ApplyBeatmap(beatmap);
        }

        protected override void OnResultApplied(JudgementResult result)
        {
            base.OnResultApplied(result);

            if (result.IsHit)
            {
                int binOffset = (int)(result.TimeOffset / timingDistribution.BinSize);
                timingDistribution.Bins[timing_distribution_centre_bin_index + binOffset]++;
            }
        }

        protected override void OnResultReverted(JudgementResult result)
        {
            base.OnResultReverted(result);

            if (result.IsHit)
            {
                int binOffset = (int)(result.TimeOffset / timingDistribution.BinSize);
                timingDistribution.Bins[timing_distribution_centre_bin_index + binOffset]--;
            }
        }

        public override void PopulateScore(ScoreInfo score)
        {
            base.PopulateScore(score);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            timingDistribution.Bins.AsSpan().Clear();
        }

        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new OsuJudgementResult(hitObject, judgement);

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }

    public class TimingDistribution
    {
        public readonly int[] Bins;
        public readonly double BinSize;

        public TimingDistribution(int binCount, double binSize)
        {
            Bins = new int[binCount];
            BinSize = binSize;
        }
    }
}
