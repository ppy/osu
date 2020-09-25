// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatistic : StatisticDisplay
    {
        private readonly ScoreInfo score;

        private readonly Bindable<int> performance = new Bindable<int>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private RollingCounter<int> counter;

        public PerformanceStatistic(ScoreInfo score)
            : base("PP")
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager, BeatmapDifficultyManager difficultyManager)
        {
            if (score.PP.HasValue)
            {
                performance.Value = (int)score.PP.Value;
            }
            else
            {
                var beatmap = beatmapManager.GetWorkingBeatmap(score.Beatmap);
                difficultyManager.CalculateScorePerformance(beatmap, score, cancellationTokenSource.Token)
                                 .ContinueWith(t => Schedule(() => performance.Value = (int)t.Result), cancellationTokenSource.Token);
            }
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.BindTo(performance);
        }

        protected override Drawable CreateContent() => counter = new Counter();

        private class Counter : RollingCounter<int>
        {
            protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

            protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
            {
                s.Font = OsuFont.Torus.With(size: 20, fixedWidth: true);
                s.Spacing = new Vector2(-2, 0);
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
