// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatistic : CounterStatistic
    {
        private readonly ScoreInfo score;

        private readonly Bindable<int> performance = new Bindable<int>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public PerformanceStatistic(ScoreInfo score)
            : base("PP", 0)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(ScorePerformanceManager performanceManager)
        {
            performanceManager.CalculatePerformanceAsync(score, cancellationTokenSource.Token)
                              .ContinueWith(t => Schedule(() => performance.Value = (int)t.Result), cancellationTokenSource.Token);
        }

        public override void Appear()
        {
            base.Appear();
            Counter.Current.BindTo(performance);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
