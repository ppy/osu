// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class DetailHeaderContainer : Container
    {
        private ProfileHeader.HasTooltipContainer totalPlayTimeTooltip;
        private ProfileHeader.OverlinedInfoContainer totalPlayTimeInfo, medalInfo, ppInfo;
        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();
        private ProfileHeader.OverlinedInfoContainer detailGlobalRank, detailCountryRank;
        private RankGraph rankGraph;

        private User user;
        public User User
        {
            get => user;
            set
            {
                if (user == value) return;
                user = value;
                updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.CommunityUserGrayGreenDarkest,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                                        totalPlayTimeTooltip = new ProfileHeader.HasTooltipContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            TooltipText = "0 hours",
                                            Child = totalPlayTimeInfo = new ProfileHeader.OverlinedInfoContainer
                                            {
                                                Title = "Total Play Time",
                                                LineColour = colours.Yellow,
                                            },
                                        },
                                        medalInfo = new ProfileHeader.OverlinedInfoContainer
                                        {
                                            Title = "Medals",
                                            LineColour = colours.GreenLight,
                                        },
                                        ppInfo = new ProfileHeader.OverlinedInfoContainer
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
                                rankGraph = new RankGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                                        detailGlobalRank = new ProfileHeader.OverlinedInfoContainer(true, 110)
                                        {
                                            Title = "Global Ranking",
                                            LineColour = colours.Yellow,
                                        },
                                        detailCountryRank = new ProfileHeader.OverlinedInfoContainer(false, 110)
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
        }

        private void updateDisplay()
        {
            medalInfo.Content = user?.Achievements?.Length.ToString() ?? "0";
            ppInfo.Content = user?.Statistics?.PP?.ToString("#,##0") ?? "0";

            string formatTime(int? secondsNull)
            {
                if (secondsNull == null) return "0h 0m";

                int seconds = secondsNull.Value;
                string time = "";

                int days = seconds / 86400;
                seconds -= days * 86400;
                if (days > 0)
                    time += days + "d ";

                int hours = seconds / 3600;
                seconds -= hours * 3600;
                time += hours + "h ";

                int minutes = seconds / 60;
                time += minutes + "m";

                return time;
            }

            totalPlayTimeInfo.Content = formatTime(user?.Statistics?.PlayTime);
            totalPlayTimeTooltip.TooltipText = (user?.Statistics?.PlayTime ?? 0) / 3600 + " hours";

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = user?.Statistics?.GradesCount.GetForScoreRank(scoreRankInfo.Key) ?? 0;

            detailGlobalRank.Content = user?.Statistics?.Ranks.Global?.ToString("#,##0") ?? "-";
            detailCountryRank.Content = user?.Statistics?.Ranks.Country?.ToString("#,##0") ?? "-";

            rankGraph.User.Value = user;
        }

        private class ScoreRankInfo : CompositeDrawable
        {
            private readonly ScoreRank rank;
            private readonly Sprite rankSprite;
            private readonly OsuSpriteText rankCount;

            public int RankCount
            {
                set => rankCount.Text = value.ToString("#,##0");
            }

            public ScoreRankInfo(ScoreRank rank)
            {
                this.rank = rank;

                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 56,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        rankSprite = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit
                        },
                        rankCount = new OsuSpriteText
                        {
                            Font = "Exo2.0-Bold",
                            TextSize = 12,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                rankSprite.Texture = textures.Get($"Grades/{rank.GetDescription()}");
            }
        }
    }
}
