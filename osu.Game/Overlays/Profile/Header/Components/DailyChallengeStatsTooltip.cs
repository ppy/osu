// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DailyChallengeStatsTooltip : VisibilityContainer, ITooltip<DailyChallengeTooltipData>
    {
        private StreakPiece currentDaily = null!;
        private StreakPiece currentWeekly = null!;
        private StatisticsPiece bestDaily = null!;
        private StatisticsPiece bestWeekly = null!;
        private StatisticsPiece topTen = null!;
        private StatisticsPiece topFifty = null!;

        private Box topBackground = null!;
        private Box background = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 20f;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Radius = 30f,
            };

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                topBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding(15f),
                                    Spacing = new Vector2(30f),
                                    Children = new[]
                                    {
                                        currentDaily = new StreakPiece(UsersStrings.ShowDailyChallengeDailyStreakCurrent),
                                        currentWeekly = new StreakPiece(UsersStrings.ShowDailyChallengeWeeklyStreakCurrent),
                                    }
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(15f),
                            Spacing = new Vector2(10f),
                            Children = new[]
                            {
                                bestDaily = new StatisticsPiece(UsersStrings.ShowDailyChallengeDailyStreakBest),
                                bestWeekly = new StatisticsPiece(UsersStrings.ShowDailyChallengeWeeklyStreakBest),
                                topTen = new StatisticsPiece(UsersStrings.ShowDailyChallengeTop10pPlacements),
                                topFifty = new StatisticsPiece(UsersStrings.ShowDailyChallengeTop50pPlacements),
                            }
                        },
                    }
                }
            };
        }

        public void SetContent(DailyChallengeTooltipData content)
        {
            var statistics = content.Statistics;
            var colourProvider = content.ColourProvider;

            background.Colour = colourProvider.Background4;
            topBackground.Colour = colourProvider.Background5;

            currentDaily.Value = DailyChallengeStatsDisplayStrings.UnitDay(content.Statistics.DailyStreakCurrent.ToLocalisableString(@"N0"));
            currentDaily.ValueColour = colours.ForRankingTier(TierForDaily(statistics.DailyStreakCurrent));

            currentWeekly.Value = DailyChallengeStatsDisplayStrings.UnitWeek(statistics.WeeklyStreakCurrent.ToLocalisableString(@"N0"));
            currentWeekly.ValueColour = colours.ForRankingTier(TierForWeekly(statistics.WeeklyStreakCurrent));

            bestDaily.Value = DailyChallengeStatsDisplayStrings.UnitDay(statistics.DailyStreakBest.ToLocalisableString(@"N0"));
            bestDaily.ValueColour = colours.ForRankingTier(TierForDaily(statistics.DailyStreakBest));

            bestWeekly.Value = DailyChallengeStatsDisplayStrings.UnitWeek(statistics.WeeklyStreakBest.ToLocalisableString(@"N0"));
            bestWeekly.ValueColour = colours.ForRankingTier(TierForWeekly(statistics.WeeklyStreakBest));

            topTen.Value = statistics.Top10PercentPlacements.ToLocalisableString(@"N0");
            topTen.ValueColour = colourProvider.Content2;

            topFifty.Value = statistics.Top50PercentPlacements.ToLocalisableString(@"N0");
            topFifty.ValueColour = colourProvider.Content2;
        }

        // reference: https://github.com/ppy/osu-web/blob/8206e0e91eeea80ccf92f0586561346dd40e085e/resources/js/profile-page/daily-challenge.tsx#L13-L43
        public static RankingTier TierForDaily(int daily)
        {
            if (daily > 360)
                return RankingTier.Lustrous;

            if (daily > 240)
                return RankingTier.Radiant;

            if (daily > 120)
                return RankingTier.Rhodium;

            if (daily > 60)
                return RankingTier.Platinum;

            if (daily > 30)
                return RankingTier.Gold;

            if (daily > 10)
                return RankingTier.Silver;

            if (daily > 5)
                return RankingTier.Bronze;

            return RankingTier.Iron;
        }

        public static RankingTier TierForWeekly(int weekly) => TierForDaily((weekly - 1) * 7);

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private partial class StreakPiece : FillFlowContainer
        {
            private readonly OsuSpriteText valueText;

            public LocalisableString Value
            {
                set => valueText.Text = value;
            }

            public ColourInfo ValueColour
            {
                set => valueText.Colour = value;
            }

            public StreakPiece(LocalisableString title)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12),
                        Text = title,
                    },
                    valueText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 40, weight: FontWeight.Light),
                    }
                };
            }
        }

        private partial class StatisticsPiece : CompositeDrawable
        {
            private readonly OsuSpriteText valueText;

            public LocalisableString Value
            {
                set => valueText.Text = value;
            }

            public ColourInfo ValueColour
            {
                set => valueText.Colour = value;
            }

            public StatisticsPiece(LocalisableString title)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12),
                        Text = title,
                    },
                    valueText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 12),
                    }
                };
            }
        }
    }

    public record DailyChallengeTooltipData(OverlayColourProvider ColourProvider, APIUserDailyChallengeStatistics Statistics);
}
