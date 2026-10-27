// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.RankingV2.Extended
{
    public partial class TimingStatisticsPanel : StatisticItemPanel
    {
        private StatisticValue averageHitErrorText = null!;
        private StatisticValue unstableRateText = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        public TimingStatisticsPanel()
            : base(ResultsScreenStrings.TimingStatisticsPanelHeader)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children =
                [
                    averageHitErrorText = new StatisticValue
                    {
                        Caption = RankingStatisticsStrings.AverageHitErrorTitle,
                    },
                    unstableRateText = new StatisticValue
                    {
                        Caption = RankingStatisticsStrings.UnstableRateTitle,
                    }
                ],
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            if (score.Value is not IScoreWithReplayData scoreWithReplayData)
            {
                Alpha = 0;
                return;
            }

            var hitEvents = scoreWithReplayData.HitEvents;

            double? averageHitError = hitEvents.CalculateAverageHitError();
            var unstableRate = hitEvents.CalculateUnstableRate();

            if (averageHitError == null || unstableRate == null || unstableRate.EventCount == 0)
            {
                Alpha = 0;
                return;
            }

            Alpha = 1;
            averageHitErrorText.Value = averageHitError.Value < 0
                ? RankingStatisticsStrings.Early(Math.Abs(averageHitError.Value))
                : RankingStatisticsStrings.Late(Math.Abs(averageHitError.Value));
            unstableRateText.Value = unstableRate.Result.ToLocalisableString(@"N2");
        }
    }
}
