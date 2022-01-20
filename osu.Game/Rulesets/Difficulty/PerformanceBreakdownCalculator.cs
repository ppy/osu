// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public class PerformanceBreakdownCalculator
    {
        private readonly BeatmapManager beatmapManager;
        private readonly BeatmapDifficultyCache difficultyCache;
        private readonly ScorePerformanceCache performanceCache;

        public PerformanceBreakdownCalculator(BeatmapManager beatmapManager, BeatmapDifficultyCache difficultyCache, ScorePerformanceCache performanceCache)
        {
            this.beatmapManager = beatmapManager;
            this.difficultyCache = difficultyCache;
            this.performanceCache = performanceCache;
        }

        [ItemCanBeNull]
        public async Task<PerformanceBreakdown> CalculateAsync(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            PerformanceAttributes[] performanceArray = await Task.WhenAll(
                // compute actual performance
                performanceCache.CalculatePerformanceAsync(score, cancellationToken),
                // compute performance for perfect play
                getPerfectPerformance(score, cancellationToken)
            ).ConfigureAwait(false);

            return new PerformanceBreakdown { Performance = performanceArray[0], PerfectPerformance = performanceArray[1] };
        }

        [ItemCanBeNull]
        private async Task<PerformanceAttributes> getPerfectPerformance(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            ScoreInfo perfectScore = await getPerfectScore(score, cancellationToken).ConfigureAwait(false);
            if (perfectScore == null)
                return null;

            return await performanceCache.CalculatePerformanceAsync(perfectScore, cancellationToken).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        private Task<ScoreInfo> getPerfectScore(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                IBeatmap beatmap = beatmapManager.GetWorkingBeatmap(score.BeatmapInfo).GetPlayableBeatmap(score.Ruleset, score.Mods);
                ScoreInfo perfectPlay = score.DeepClone();
                perfectPlay.Accuracy = 1;
                perfectPlay.Passed = true;

                // calculate max combo
                var difficulty = await difficultyCache.GetDifficultyAsync(
                    beatmap.BeatmapInfo,
                    score.Ruleset,
                    score.Mods,
                    cancellationToken
                ).ConfigureAwait(false);

                if (difficulty == null)
                    return null;

                perfectPlay.MaxCombo = difficulty.Value.MaxCombo;

                // create statistics assuming all hit objects have perfect hit result
                var statistics = beatmap.HitObjects
                                        .Select(ho => ho.CreateJudgement().MaxResult)
                                        .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                        .ToDictionary(pair => pair.hitResult, pair => pair.count);
                perfectPlay.Statistics = statistics;

                // calculate total score
                ScoreProcessor scoreProcessor = score.Ruleset.CreateInstance().CreateScoreProcessor();
                scoreProcessor.HighestCombo.Value = perfectPlay.MaxCombo;
                scoreProcessor.Mods.Value = perfectPlay.Mods;
                perfectPlay.TotalScore = (long)scoreProcessor.GetImmediateScore(ScoringMode.Standardised, perfectPlay.MaxCombo, statistics);

                // compute rank achieved
                // default to SS, then adjust the rank with mods
                perfectPlay.Rank = ScoreRank.X;

                foreach (IApplicableToScoreProcessor mod in perfectPlay.Mods.OfType<IApplicableToScoreProcessor>())
                {
                    perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
                }

                return perfectPlay;
            }, cancellationToken);
        }
    }
}
