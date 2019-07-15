// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class DetailHeaderContainer : CompositeDrawable
    {
        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();
        private OverlinedInfoContainer medalInfo;
        private OverlinedInfoContainer ppInfo;
        private OverlinedInfoContainer detailGlobalRank;
        private OverlinedInfoContainer detailCountryRank;
        private FillFlowContainer fillFlow;

        public readonly Bindable<User> User = new Bindable<User>();
        public readonly Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        private bool expanded = true;

        public bool Expanded
        {
            set
            {
                if (expanded == value) return;

                expanded = value;

                if (fillFlow == null) return;

                fillFlow.ClearTransforms();

                if (expanded)
                    fillFlow.AutoSizeAxes = Axes.Y;
                else
                {
                    fillFlow.AutoSizeAxes = Axes.None;
                    fillFlow.ResizeHeightTo(0, 200, Easing.OutQuint);
                }
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreySeafoamDarker,
                },
                fillFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = expanded ? Axes.Y : Axes.None,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new OverlinedTotalPlayTime
                                        {
                                            Statistics = { BindTarget = Statistics },
                                        },
                                        medalInfo = new OverlinedInfoContainer
                                        {
                                            Title = "Medals",
                                            LineColour = colours.GreenLight,
                                        },
                                        ppInfo = new OverlinedInfoContainer
                                        {
                                            Title = "pp",
                                            LineColour = colours.Red,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    Children = new[]
                                    {
                                        scoreRankInfos[ScoreRank.XH] = new ScoreRankInfo(ScoreRank.XH),
                                        scoreRankInfos[ScoreRank.X] = new ScoreRankInfo(ScoreRank.X),
                                        scoreRankInfos[ScoreRank.SH] = new ScoreRankInfo(ScoreRank.SH),
                                        scoreRankInfos[ScoreRank.S] = new ScoreRankInfo(ScoreRank.S),
                                        scoreRankInfos[ScoreRank.A] = new ScoreRankInfo(ScoreRank.A),
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 130 },
                            Children = new Drawable[]
                            {
                                new RankGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Statistics = { BindTarget = Statistics },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Y,
                                    Width = 130,
                                    Anchor = Anchor.TopRight,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = 10 },
                                    Spacing = new Vector2(0, 20),
                                    Children = new Drawable[]
                                    {
                                        detailGlobalRank = new OverlinedInfoContainer(true, 110)
                                        {
                                            Title = "Global Ranking",
                                            LineColour = colours.Yellow,
                                        },
                                        detailCountryRank = new OverlinedInfoContainer(false, 110)
                                        {
                                            Title = "Country Ranking",
                                            LineColour = colours.Yellow,
                                        },
                                    }
                                }
                            }
                        },
                    }
                },
            };

            User.BindValueChanged(user => onUserUpdate(user.NewValue));
            Statistics.BindValueChanged(statistics => onStatisticsUpdate(statistics.NewValue));
        }

        private void onUserUpdate(User user)
        {
            medalInfo.Content = user?.Achievements?.Length.ToString() ?? "0";
        }

        private void onStatisticsUpdate(UserStatistics statistics)
        {
            ppInfo.Content = statistics?.PP?.ToString("#,##0") ?? "0";

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = statistics?.GradesCount[scoreRankInfo.Key] ?? 0;

            detailGlobalRank.Content = statistics?.Ranks.Global?.ToString("\\##,##0") ?? "-";
            detailCountryRank.Content = statistics?.Ranks.Country?.ToString("\\##,##0") ?? "-";
        }

        private class ScoreRankInfo : CompositeDrawable
        {
            private readonly OsuSpriteText rankCount;

            public int RankCount
            {
                set => rankCount.Text = value.ToString("#,##0");
            }

            public ScoreRankInfo(ScoreRank rank)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 56,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new DrawableRank(rank)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                        },
                        rankCount = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    }
                };
            }
        }
    }
}
