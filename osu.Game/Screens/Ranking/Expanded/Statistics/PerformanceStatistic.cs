// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatistic : StatisticDisplay, IHasCustomTooltip<PerformanceStatistic.PerformanceBreakdown>
    {
        private readonly ScoreInfo score;

        private readonly Bindable<int> performance = new Bindable<int>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private RollingCounter<int> counter;

        [Resolved]
        private ScorePerformanceCache performanceCache { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public PerformanceStatistic(ScoreInfo score)
            : base("PP")
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Task.WhenAll(
                // actual performance
                performanceCache.CalculatePerformanceAsync(score, cancellationTokenSource.Token),
                // performance for a perfect play
                getPerfectPerformance(score)
            ).ContinueWith(attr =>
            {
                PerformanceAttributes[] result = attr.GetResultSafely();
                setPerformanceValue(new PerformanceBreakdown { Performance = result[0], PerfectPerformance = result[1] });
            });
        }

        private async Task<PerformanceAttributes> getPerfectPerformance(ScoreInfo originalScore)
        {
            ScoreInfo perfectScore = await getPerfectScore(originalScore).ConfigureAwait(false);
            return await performanceCache.CalculatePerformanceAsync(perfectScore, cancellationTokenSource.Token).ConfigureAwait(false);
        }

        private Task<ScoreInfo> getPerfectScore(ScoreInfo originalScore)
        {
            return Task.Factory.StartNew(() =>
            {
                var beatmap = beatmapManager.GetWorkingBeatmap(originalScore.BeatmapInfo).GetPlayableBeatmap(originalScore.Ruleset, originalScore.Mods);
                ScoreInfo perfectPlay = originalScore.DeepClone();
                perfectPlay.Accuracy = 1;
                perfectPlay.Passed = true;

                // create statistics assuming all hit objects have perfect hit result
                var statistics = beatmap.HitObjects
                                        .Select(ho => ho.CreateJudgement().MaxResult)
                                        .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                        .ToDictionary(pair => pair.hitResult, pair => pair.count);
                perfectPlay.Statistics = statistics;

                // calculate max combo
                var difficulty = difficultyCache.GetDifficultyAsync(
                    beatmap.BeatmapInfo,
                    originalScore.Ruleset,
                    originalScore.Mods,
                    cancellationTokenSource.Token
                ).GetResultSafely();
                perfectPlay.MaxCombo = difficulty?.MaxCombo ?? originalScore.MaxCombo;

                // calculate total score
                ScoreProcessor scoreProcessor = originalScore.Ruleset.CreateInstance().CreateScoreProcessor();
                perfectPlay.TotalScore = (long)scoreProcessor.GetImmediateScore(ScoringMode.Standardised, perfectPlay.MaxCombo, statistics);

                // compute rank achieved
                // default to SS, then adjust the rank with mods
                perfectPlay.Rank = ScoreRank.X;

                foreach (IApplicableToScoreProcessor mod in perfectPlay.Mods.OfType<IApplicableToScoreProcessor>())
                {
                    perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
                }

                return perfectPlay;
            }, cancellationTokenSource.Token);
        }

        private void setPerformanceValue(PerformanceBreakdown breakdown)
        {
            if (breakdown != null)
            {
                TooltipContent = breakdown;
                performance.Value = (int)Math.Round(breakdown.Performance.Total, MidpointRounding.AwayFromZero);
            }
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.BindTo(performance);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource?.Cancel();
            base.Dispose(isDisposing);
        }

        protected override Drawable CreateContent() => counter = new StatisticCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre
        };

        public ITooltip<PerformanceBreakdown> GetCustomTooltip() => new PerformanceStatisticTooltip();

        public PerformanceBreakdown TooltipContent { get; private set; }

        public class PerformanceBreakdown
        {
            /// <summary>
            /// Actual gameplay performance.
            /// </summary>
            public PerformanceAttributes Performance { get; set; }

            /// <summary>
            /// Performance of a perfect play for comparison.
            /// </summary>
            public PerformanceAttributes PerfectPerformance { get; set; }
        }
    }
}
