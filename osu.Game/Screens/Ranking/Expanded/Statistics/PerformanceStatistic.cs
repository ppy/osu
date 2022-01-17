// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatistic : StatisticDisplay, IHasCustomTooltip<PerformanceAttributes>
    {
        private readonly ScoreInfo score;

        private readonly Bindable<int> performance = new Bindable<int>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private RollingCounter<int> counter;

        private PerformanceAttributes attributes;

        public PerformanceStatistic(ScoreInfo score)
            : base("PP")
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(ScorePerformanceCache performanceCache)
        {
            performanceCache.CalculatePerformanceAsync(score, cancellationTokenSource.Token)
                            .ContinueWith(t => Schedule(() => setPerformanceValue(t.GetResultSafely())), cancellationTokenSource.Token);
        }

        private void setPerformanceValue(PerformanceAttributes pp)
        {
            if (pp != null)
            {
                attributes = pp;
                performance.Value = (int)Math.Round(pp.Total, MidpointRounding.AwayFromZero);
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

        public ITooltip<PerformanceAttributes> GetCustomTooltip() => new PerformanceStatisticTooltip();

        public PerformanceAttributes TooltipContent => attributes;
    }
}
