// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public class PerformanceBreakdownCalculator
    {
        private readonly IBeatmap playableBeatmap;
        private readonly BeatmapDifficultyCache difficultyCache;
        private readonly ScorePerformanceCache performanceCache;

        public PerformanceBreakdownCalculator(IBeatmap playableBeatmap, BeatmapDifficultyCache difficultyCache, ScorePerformanceCache performanceCache)
        {
            this.playableBeatmap = playableBeatmap;
            this.difficultyCache = difficultyCache;
            this.performanceCache = performanceCache;
        }

        [ItemCanBeNull]
        public async Task<PerformanceBreakdown> CalculateAsync(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            PerformanceAttributes[] performanceArray = await Task.WhenAll(
                // compute actual performance
                performanceCache.CalculatePerformanceAsync(score, cancellationToken),
                // compute performance for a full combo
                getFCPerformance(score, cancellationToken),
                // compute performance for perfect play
                getPerfectPerformance(score, cancellationToken)
            ).ConfigureAwait(false);

            return new PerformanceBreakdown(performanceArray[0] ?? new PerformanceAttributes(), performanceArray[1] ?? new PerformanceAttributes(), performanceArray[2] ?? new PerformanceAttributes());
        }

        [ItemCanBeNull]
        private Task<PerformanceAttributes> getFCPerformance(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                Ruleset rulest = score.Ruleset.CreateInstance();
                ScoreInfo fcPlay = score.DeepClone();
                fcPlay.Passed = true;
                // Update the play to be a full combo
                fcPlay.MaxCombo = calculateMaxCombo(playableBeatmap);

                // Only recalculate accuracy if it's different
                if (fcPlay.Statistics[HitResult.Miss] > 0)
                {
                    // number of hits which are worth 300 or more score (greats and perfects)
                    int count300 = fcPlay.Statistics.Where(x => Judgement.ToNumericResult(x.Key) >= 300).Sum(x => x.Value);
                    // The ratio of 300s to other values not including misses
                    double ratio300 = (double)count300 / (fcPlay.Statistics.Sum(x => x.Value) - fcPlay.Statistics[HitResult.Miss]);
                    // The number of extra greats to add
                    int newGreat = (int)Math.Round(fcPlay.Statistics[HitResult.Miss] * ratio300);
                    // The number of extra oks to add
                    int newOk = fcPlay.Statistics[HitResult.Miss] - newGreat;
                    fcPlay.Statistics[HitResult.Great] += newGreat;
                    fcPlay.Statistics[HitResult.Ok] += newOk;
                    fcPlay.Statistics[HitResult.Miss] = 0;
                    // Recalculate Accuracy with misses converted to 300s
                    fcPlay.CalculateAccuracy();
                }

                var difficulty = await difficultyCache.GetDifficultyAsync(
                    playableBeatmap.BeatmapInfo,
                    score.Ruleset,
                    score.Mods,
                    cancellationToken
                ).ConfigureAwait(false);

                return difficulty == null ? null : rulest.CreatePerformanceCalculator()?.Calculate(fcPlay, difficulty.Value.Attributes.AsNonNull());
            }, cancellationToken);
        }

        [ItemCanBeNull]
        private Task<PerformanceAttributes> getPerfectPerformance(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                Ruleset ruleset = score.Ruleset.CreateInstance();
                ScoreInfo perfectPlay = score.DeepClone();
                perfectPlay.Accuracy = 1;
                perfectPlay.Passed = true;

                // calculate max combo
                // todo: Get max combo from difficulty calculator instead when diffcalc properly supports lazer-first scores
                perfectPlay.MaxCombo = calculateMaxCombo(playableBeatmap);

                // create statistics assuming all hit objects have perfect hit result
                var statistics = playableBeatmap.HitObjects
                                                .SelectMany(getPerfectHitResults)
                                                .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                                .ToDictionary(pair => pair.hitResult, pair => pair.count);
                perfectPlay.Statistics = statistics;
                perfectPlay.MaximumStatistics = statistics;

                // calculate total score
                ScoreProcessor scoreProcessor = ruleset.CreateScoreProcessor();
                scoreProcessor.Mods.Value = perfectPlay.Mods;
                scoreProcessor.ApplyBeatmap(playableBeatmap);
                perfectPlay.TotalScore = scoreProcessor.MaximumTotalScore;

                // compute rank achieved
                // default to SS, then adjust the rank with mods
                perfectPlay.Rank = ScoreRank.X;

                foreach (IApplicableToScoreProcessor mod in perfectPlay.Mods.OfType<IApplicableToScoreProcessor>())
                {
                    perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
                }

                // calculate performance for this perfect score
                var difficulty = await difficultyCache.GetDifficultyAsync(
                    playableBeatmap.BeatmapInfo,
                    score.Ruleset,
                    score.Mods,
                    cancellationToken
                ).ConfigureAwait(false);

                // ScorePerformanceCache is not used to avoid caching multiple copies of essentially identical perfect performance attributes
                return difficulty == null ? null : ruleset.CreatePerformanceCalculator()?.Calculate(perfectPlay, difficulty.Value.Attributes.AsNonNull());
            }, cancellationToken);
        }

        private int calculateMaxCombo(IBeatmap beatmap)
        {
            return beatmap.HitObjects.SelectMany(getPerfectHitResults).Count(r => r.AffectsCombo());
        }

        private IEnumerable<HitResult> getPerfectHitResults(HitObject hitObject)
        {
            foreach (HitObject nested in hitObject.NestedHitObjects)
                yield return nested.CreateJudgement().MaxResult;

            yield return hitObject.CreateJudgement().MaxResult;
        }
    }
}
