// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class OverallRanking : CompositeDrawable
    {
        private const float transition_duration = 300;

        public Bindable<ScoreBasedUserStatisticsUpdate?> DisplayedUpdate { get; } = new Bindable<ScoreBasedUserStatisticsUpdate?>();
        private readonly IBindable<ScoreBasedUserStatisticsUpdate?> latestGlobalStatisticsUpdate = new Bindable<ScoreBasedUserStatisticsUpdate?>();

        private readonly ScoreInfo scoreInfo;

        private LoadingLayer loadingLayer = null!;
        private GridContainer content = null!;

        public OverallRanking(ScoreInfo scoreInfo)
        {
            this.scoreInfo = scoreInfo;
        }

        [BackgroundDependencyLoader]
        private void load(UserStatisticsWatcher? userStatisticsWatcher)
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
                            new GlobalRankChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new PerformancePointsChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                        },
                        [],
                        new Drawable[]
                        {
                            new MaximumComboChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new AccuracyChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                        },
                        [],
                        new Drawable[]
                        {
                            new RankedScoreChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                            new SimpleStatisticTable.Spacer(),
                            new TotalScoreChangeRow { StatisticsUpdate = { BindTarget = DisplayedUpdate } },
                        }
                    }
                }
            };

            if (userStatisticsWatcher != null)
            {
                latestGlobalStatisticsUpdate.BindTo(userStatisticsWatcher.LatestUpdate);
                latestGlobalStatisticsUpdate.BindValueChanged(update =>
                {
                    if (update.NewValue?.Score.MatchesOnlineID(scoreInfo) == true)
                        DisplayedUpdate.Value = update.NewValue;
                }, true);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedUpdate.BindValueChanged(onUpdateReceived, true);
            FinishTransforms(true);
        }

        private void onUpdateReceived(ValueChangedEvent<ScoreBasedUserStatisticsUpdate?> update)
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
