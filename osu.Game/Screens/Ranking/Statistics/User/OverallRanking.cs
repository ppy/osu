// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class OverallRanking : CompositeDrawable
    {
        private const float transition_duration = 300;

        public Bindable<UserStatisticsUpdate?> StatisticsUpdate { get; } = new Bindable<UserStatisticsUpdate?>();

        private LoadingLayer loadingLayer = null!;
        private GridContainer content = null!;

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
                content = new GridContainer
                {
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 30),
                        new Dimension(),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new GlobalRankChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new PerformancePointsChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        },
                        new Drawable[] { },
                        new Drawable[]
                        {
                            new MaximumComboChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new AccuracyChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        },
                        new Drawable[] { },
                        new Drawable[]
                        {
                            new RankedScoreChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new TotalScoreChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            StatisticsUpdate.BindValueChanged(onUpdateReceived, true);
            FinishTransforms(true);
        }

        private void onUpdateReceived(ValueChangedEvent<UserStatisticsUpdate?> update)
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
