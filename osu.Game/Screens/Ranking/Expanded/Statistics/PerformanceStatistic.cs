﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public partial class PerformanceStatistic : StatisticDisplay
    {
        private readonly ScoreInfo score;

        private readonly Bindable<int> performance = new Bindable<int>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private RollingCounter<int> counter;

        public PerformanceStatistic(ScoreInfo score)
            : base(BeatmapsetsStrings.ShowScoreboardHeaderspp)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(ScorePerformanceCache performanceCache)
        {
            if (score.PP.HasValue)
            {
                setPerformanceValue(score.PP.Value);
            }
            else
            {
                performanceCache.CalculatePerformanceAsync(score, cancellationTokenSource.Token)
                                .ContinueWith(t => Schedule(() => setPerformanceValue(t.GetResultSafely()?.Total)), cancellationTokenSource.Token);
            }
        }

        private void setPerformanceValue(double? pp)
        {
            if (pp.HasValue)
                performance.Value = (int)Math.Round(pp.Value, MidpointRounding.AwayFromZero);
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
    }
}
