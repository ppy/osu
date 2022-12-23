// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class OverallRanking : CompositeDrawable
    {
        private const float transition_duration = 300;

        private readonly ScoreInfo score;

        private readonly Bindable<SoloStatisticsUpdate?> statisticsUpdate = new Bindable<SoloStatisticsUpdate?>();

        private LoadingLayer loadingLayer = null!;
        private FillFlowContainer content = null!;

        [Resolved]
        private ISoloStatisticsWatcher statisticsWatcher { get; set; } = null!;

        public OverallRanking(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = transition_duration;

            InternalChildren = new Drawable[]
            {
                loadingLayer = new LoadingLayer(withBox: false)
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new FillFlowContainer
                {
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new GlobalRankChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } },
                        new AccuracyChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } },
                        new MaximumComboChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } },
                        new RankedScoreChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } },
                        new TotalScoreChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } },
                        new PerformancePointsChangeRow { StatisticsUpdate = { BindTarget = statisticsUpdate } }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            statisticsWatcher.RegisterForStatisticsUpdateAfter(score, update => statisticsUpdate.Value = update);
            statisticsUpdate.BindValueChanged(onUpdateReceived, true);
            FinishTransforms(true);
        }

        private void onUpdateReceived(ValueChangedEvent<SoloStatisticsUpdate?> update)
        {
            if (update.NewValue == null)
            {
                loadingLayer.Show();
                content.FadeOut(transition_duration, Easing.OutQuint);
            }
            else
            {
                loadingLayer.Hide();
                content.FadeIn(transition_duration, Easing.OutQuint);
            }
        }
    }
}
