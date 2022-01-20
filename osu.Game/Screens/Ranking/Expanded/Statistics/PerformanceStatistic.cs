// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatistic : StatisticDisplay, IHasCustomTooltip<PerformanceBreakdown>
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
            new PerformanceBreakdownCalculator(beatmapManager, difficultyCache, performanceCache)
                .CalculateAsync(score, cancellationTokenSource.Token)
                .ContinueWith(t => setPerformanceValue(t.GetResultSafely()));
        }

        private void setPerformanceValue(PerformanceBreakdown breakdown)
        {
            // Don't display the tooltip if "Total" is the only item
            if (breakdown != null && breakdown.Performance.GetAttributesForDisplay().Count() > 1)
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
    }
}
