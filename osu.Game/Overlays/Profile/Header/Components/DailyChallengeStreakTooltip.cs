// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DailyChallengeStreakTooltip : VisibilityContainer, ITooltip<APIUserDailyChallengeStatistics>
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider;

        private StreakPiece currentDaily = null!;
        private StreakPiece currentWeekly = null!;
        private StatisticsPiece bestDaily = null!;
        private StatisticsPiece bestWeekly = null!;
        private StatisticsPiece topTen = null!;
        private StatisticsPiece topFifty = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public DailyChallengeStreakTooltip(OverlayColourProvider colourProvider)
        {
            this.colourProvider = colourProvider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 20f;
            Masking = true;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
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
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding(15f),
                                    Spacing = new Vector2(30f),
                                    Children = new[]
                                    {
                                        // currentDaily = new StreakPiece(UsersStrings.ShowDailyChallengeDailyStreakCurrent),
                                        // currentWeekly = new StreakPiece(UsersStrings.ShowDailyChallengeWeeklyStreakCurrent),
                                        currentDaily = new StreakPiece("Current Daily Streak"),
                                        currentWeekly = new StreakPiece("Current Weekly Streak"),
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
                                // bestDaily = new StatisticsPiece(UsersStrings.ShowDailyChallengeDailyStreakBest),
                                // bestWeekly = new StatisticsPiece(UsersStrings.ShowDailyChallengeWeeklyStreakBest),
                                // topTen = new StatisticsPiece(UsersStrings.ShowDailyChallengeTop10pPlacements),
                                // topFifty = new StatisticsPiece(UsersStrings.ShowDailyChallengeTop50pPlacements),
                                bestDaily = new StatisticsPiece("Best Daily Streak"),
                                bestWeekly = new StatisticsPiece("Best Weekly Streak"),
                                topTen = new StatisticsPiece("Top 10% Placements"),
                                topFifty = new StatisticsPiece("Top 50% Placements"),
                            }
                        },
                    }
                }
            };
        }

        public void SetContent(APIUserDailyChallengeStatistics content)
        {
            // currentDaily.Value = UsersStrings.ShowDailyChallengeUnitDay(content.DailyStreakCurrent.ToLocalisableString(@"N0"));
            currentDaily.Value = $"{content.DailyStreakCurrent:N0}d";
            currentDaily.ValueColour = colours.ForRankingTier(TierForDaily(content.DailyStreakCurrent));

            // currentWeekly.Value = UsersStrings.ShowDailyChallengeUnitWeek(content.WeeklyStreakCurrent.ToLocalisableString(@"N0"));
            currentWeekly.Value = $"{content.WeeklyStreakCurrent:N0}w";
            currentWeekly.ValueColour = colours.ForRankingTier(TierForWeekly(content.WeeklyStreakCurrent));

            // bestDaily.Value = UsersStrings.ShowDailyChallengeUnitDay(content.DailyStreakBest.ToLocalisableString(@"N0"));
            bestDaily.Value = $"{content.DailyStreakBest:N0}d";
            bestDaily.ValueColour = colours.ForRankingTier(TierForDaily(content.DailyStreakBest));

            // bestWeekly.Value = UsersStrings.ShowDailyChallengeUnitWeek(content.WeeklyStreakBest.ToLocalisableString(@"N0"));
            bestWeekly.Value = $"{content.WeeklyStreakBest:N0}w";
            bestWeekly.ValueColour = colours.ForRankingTier(TierForWeekly(content.WeeklyStreakBest));

            topTen.Value = content.Top10PercentPlacements.ToLocalisableString(@"N0");
            topFifty.Value = content.Top50PercentPlacements.ToLocalisableString(@"N0");
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
                        // Colour = colour
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

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                valueText.Colour = colourProvider.Content2;
            }
        }
    }
}
