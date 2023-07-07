// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Solo;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class OverallRanking : CompositeDrawable
    {
        private const float transition_duration = 300;

        public Bindable<SoloStatisticsUpdate?> StatisticsUpdate { get; } = new Bindable<SoloStatisticsUpdate?>();

        private LoadingLayer loadingLayer = null!;
        private FillFlowContainer content = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
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
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new GlobalRankChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        new AccuracyChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        new MaximumComboChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        new RankedScoreChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        new TotalScoreChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } },
                        new PerformancePointsChangeRow { StatisticsUpdate = { BindTarget = StatisticsUpdate } }
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
