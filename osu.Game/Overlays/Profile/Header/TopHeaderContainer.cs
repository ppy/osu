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
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Header
{
    public class TopHeaderContainer : CompositeDrawable
    {
        private const float avatar_size = 150;

        public readonly Bindable<User> User = new Bindable<User>();

        private SupporterIcon supporterTag;
        private UpdateableAvatar avatar;
        private OsuSpriteText usernameText;
        private ExternalLinkButton openUserExternally;
        private UpdateableFlag userFlag;
        private FillFlowContainer userStats;
        private readonly Dictionary<ScoreRank, DetailHeaderContainer.ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, DetailHeaderContainer.ScoreRankInfo>();

        private PlayerStatBox medalInfo;
        private PlayerStatBox ppInfo;
        private PlayerStatBox levelInfo;
        private OsuScrollContainer scoreRankInfoScroll;
        private OsuScrollContainer userNameScroll;

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = 350;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new Container
                {
                    Padding = new MarginPadding {Vertical = Height * 0.1f, Horizontal = UserProfileOverlay.CONTENT_X_MARGIN },
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Height = 0.65f,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Masking = true,
                                            CornerRadius = 25f,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Padding = new MarginPadding{ Bottom = 4.5f },
                                                    Child = new GridContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        ColumnDimensions = new[]
                                                        {
                                                            new Dimension(),
                                                            new Dimension(),
                                                        },
                                                        Content = new[]
                                                        {
                                                            new Drawable[]
                                                            {
                                                                new OverlinedTotalPlayTime
                                                                {
                                                                    User = { BindTarget = User }
                                                                },
                                                                medalInfo = new PlayerStatBox
                                                                {
                                                                    Icon = FontAwesome.Solid.Medal,
                                                                    Title = "奖章数"
                                                                },
                                                            },
                                                            new Drawable[]
                                                            {
                                                                ppInfo = new PlayerStatBox(0)
                                                                {
                                                                    IconDescription = "PP",
                                                                    Title = "pp"
                                                                },
                                                                new Container
                                                                {
                                                                    Name = "Level Bar Area",
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Children = new Drawable[]
                                                                    {
                                                                        levelInfo = new PlayerStatBox(0)
                                                                        {
                                                                            IconDescription = "EXP",
                                                                            Title = "经验"
                                                                        },
                                                                    }
                                                                },
                                                            },
                                                        }
                                                    },
                                                },
                                                new LevelProgressBar
                                                {
                                                    Anchor = Anchor.BottomCentre,
                                                    Origin = Anchor.BottomCentre,
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = 4.5f,
                                                    User = { BindTarget = User }
                                                }
                                            }
                                        },
                                        new ComponentContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Height = 0.3f,
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            Child = scoreRankInfoScroll = new OsuScrollContainer(Direction.Horizontal)
                                            {
                                                ScrollbarVisible = false,
                                                RelativeSizeAxes = Axes.Both,
                                                Child = new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Spacing = new Vector2(5),
                                                    Children = new[]
                                                    {
                                                        scoreRankInfos[ScoreRank.XH] = new DetailHeaderContainer.ScoreRankInfo(ScoreRank.XH),
                                                        scoreRankInfos[ScoreRank.X] = new DetailHeaderContainer.ScoreRankInfo(ScoreRank.X),
                                                        scoreRankInfos[ScoreRank.SH] = new DetailHeaderContainer.ScoreRankInfo(ScoreRank.SH),
                                                        scoreRankInfos[ScoreRank.S] = new DetailHeaderContainer.ScoreRankInfo(ScoreRank.S),
                                                        scoreRankInfos[ScoreRank.A] = new DetailHeaderContainer.ScoreRankInfo(ScoreRank.A),
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                                new ComponentContainer
                                {
                                    Masking= true,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.9f,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding{ Horizontal = 20, Top = 60 },//将Top设为60以临时对付对齐问题, 需要修复
                                            Spacing = new Vector2(10),
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Name = "Avatar Container",
                                                    Size = new Vector2(avatar_size),
                                                    Masking = true,
                                                    CornerRadius = avatar_size * 0.25f,
                                                    Children = new Drawable[]
                                                    {
                                                        avatar = new UpdateableAvatar
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            RelativeSizeAxes = Axes.Both,
                                                            OpenOnClick = { Value = false },
                                                            ShowGuestOnNull = false,
                                                        },
                                                        supporterTag = new SupporterIcon
                                                        {
                                                            Height = 20,
                                                            Anchor = Anchor.BottomRight,
                                                            Origin = Anchor.BottomRight,
                                                        },
                                                    }
                                                },
                                                userNameScroll = new OsuScrollContainer(Direction.Horizontal)
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    ScrollbarVisible = false,
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = 35,
                                                    Child = new FillFlowContainer
                                                    {
                                                        Name = "User Name FillFlow",
                                                        Spacing = new Vector2(7.5f),
                                                        Direction = FillDirection.Horizontal,
                                                        AutoSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Children = new Drawable[]
                                                        {
                                                            userFlag = new UpdateableFlag
                                                            {
                                                                Anchor = Anchor.Centre,
                                                                Origin = Anchor.Centre,
                                                                Size = new Vector2(30, 20),
                                                                ShowPlaceholderOnNull = false,
                                                            },
                                                            usernameText = new OsuSpriteText
                                                            {
                                                                Anchor = Anchor.Centre,
                                                                Origin = Anchor.Centre,
                                                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.Regular)
                                                            },
                                                            openUserExternally = new ExternalLinkButton
                                                            {
                                                                Anchor = Anchor.Centre,
                                                                Origin = Anchor.Centre,
                                                            },
                                                        }
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    Name = "Buttons FillFlow",
                                                    Spacing = new Vector2(10f),
                                                    Direction = FillDirection.Horizontal,
                                                    AutoSizeAxes = Axes.X,
                                                    Height = 40,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Children = new Drawable[]
                                                    {
                                                        new AddFriendButton
                                                        {
                                                            RelativeSizeAxes = Axes.Y,
                                                            User = { BindTarget = User }
                                                        },
                                                        new MessageUserButton
                                                        {
                                                            User = { BindTarget = User }
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new ComponentContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Child = userStats = new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Padding = new MarginPadding(15),
                                        Spacing = new Vector2(0, 2)
                                    }
                                }
                            }
                        }
                    },
                }
            };

            userNameScroll.ScrollContent.Anchor = Anchor.Centre;
            userNameScroll.ScrollContent.Origin = Anchor.Centre;

            scoreRankInfoScroll.ScrollContent.Anchor = Anchor.Centre;
            scoreRankInfoScroll.ScrollContent.Origin = Anchor.Centre;

            User.BindValueChanged(user => updateUser(user.NewValue));
        }

        private void updateUser(User user)
        {
            avatar.User = user;
            usernameText.Text = user?.Username ?? string.Empty;
            openUserExternally.Link = $@"https://osu.ppy.sh/users/{user?.Id ?? 0}";
            userFlag.Country = user?.Country;
            supporterTag.SupportLevel = user?.SupportLevel ?? 0;

            userStats.Clear();

            if (user?.Statistics != null)
            {
                userStats.Add(new UserStatsLine("Ranked谱面总分", user.Statistics.RankedScore.ToString("#,##0")));
                userStats.Add(new UserStatsLine("准确率", user.Statistics.DisplayAccuracy));
                userStats.Add(new UserStatsLine("游玩次数", user.Statistics.PlayCount.ToString("#,##0")));
                userStats.Add(new UserStatsLine("总分", user.Statistics.TotalScore.ToString("#,##0")));
                userStats.Add(new UserStatsLine("总连击", user.Statistics.TotalHits.ToString("#,##0")));
                userStats.Add(new UserStatsLine("最大连击", user.Statistics.MaxCombo.ToString("#,##0")));
                userStats.Add(new UserStatsLine("回放被观看次数", user.Statistics.ReplaysWatched.ToString("#,##0")));
                
                foreach (var scoreRankInfo in scoreRankInfos)
                    scoreRankInfo.Value.RankCount = user?.Statistics?.GradesCount[scoreRankInfo.Key] ?? 0;

                medalInfo.ContentText = user?.Achievements?.Length.ToString() ?? "0";
                ppInfo.ContentText = user?.Statistics?.PP?.ToString("#,##0") ?? "0";

                var levelProgress = user?.Statistics?.Level.Progress.ToString("0'%'");
                levelInfo.ContentText = $"等级{user?.Statistics?.Level.Current.ToString() ?? "0"}, 进度{levelProgress}";
            }
        }

        private class UserStatsLine : Container
        {
            public UserStatsLine(string left, string right)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 20),
                        Text = left,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                        Text = right,
                    },
                };
            }
        }
    }
}
