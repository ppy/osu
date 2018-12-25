// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : Container
    {
        private readonly RankGraph rankGraph;

        public readonly SupporterIcon SupporterTag;
        private readonly Container coverContainer;
        private readonly OsuSpriteText coverInfoText;
        private readonly ProfileHeaderTabControl infoTabControl;

        private readonly Box headerTopBox;
        private readonly UpdateableAvatar avatar;
        private readonly OsuSpriteText usernameText;
        private readonly ExternalLinkButton openUserExternally;
        private readonly OsuSpriteText titleText;
        private readonly DrawableFlag userFlag;
        private readonly OsuSpriteText userCountryText;
        private readonly Box userIconSeperatorBox;
        private readonly FillFlowContainer userStats;

        private readonly Box headerCenterBox;
        private readonly OsuSpriteText followerText;
        private readonly ProfileHeaderButton messageButton;
        private readonly ProfileHeaderButton expandButton;
        private readonly Sprite levelBadgeSprite;
        private readonly OsuSpriteText levelBadgeText;

        private readonly Bar levelProgressBar;
        private readonly OsuSpriteText levelProgressText;

        private readonly OverlinedInfoContainer hiddenDetailGlobal, hiddenDetailCountry;

        public readonly BindableBool DetailsVisible = new BindableBool();

        private readonly Box headerDetailBox;
        private readonly HasTooltipContainer totalPlayTimeTooltip;
        private readonly OverlinedInfoContainer totalPlayTimeInfo, medalInfo, ppInfo;
        private readonly Dictionary<ScoreRank, ScoreRankInfo> scoreRankInfos = new Dictionary<ScoreRank, ScoreRankInfo>();
        private readonly OverlinedInfoContainer detailGlobalRank, detailCountryRank;

        private readonly Box headerBadgeBox;
        private readonly FillFlowContainer badgeFlowContainer;
        private readonly Container badgeContainer;

        private readonly Box headerBottomBox;
        private readonly LinkFlowContainer bottomTopLinkContainer;
        private readonly LinkFlowContainer bottomLinkContainer;

        private const float cover_height = 150;
        private const float cover_info_height = 75;
        private const float avatar_size = 110;
        private static readonly Dictionary<string, string> play_styles = new Dictionary<string, string>
        {
            {"keyboard", "Keyboard"},
            {"mouse", "Mouse"},
            {"tablet", "Tablet"},
            {"touch", "Touch Screen"},
        };

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay userOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private APIAccess apiAccess { get; set; }

        public ProfileHeader()
        {
            Container expandedDetailContainer;
            FillFlowContainer hiddenDetailContainer, headerDetailContainer;
            SpriteIcon expandButtonIcon;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                    }
                },
                new Container
                {
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Y = cover_height,
                    Height = cover_info_height,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = -float.MaxValue,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Player ",
                                    Font = "Exo2.0-Regular",
                                    TextSize = 30
                                },
                                coverInfoText = new OsuSpriteText
                                {
                                    Text = "Info",
                                    Font = "Exo2.0-Regular",
                                    TextSize = 30
                                }
                            }
                        },
                        infoTabControl = new ProfileHeaderTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = cover_info_height - 30,
                            Margin = new MarginPadding { Left = -UserProfileOverlay.CONTENT_X_MARGIN },
                            Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN }
                        }
                    }
                },
                new FillFlowContainer
                {
                    Margin = new MarginPadding { Top = cover_height },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 150,
                            Children = new Drawable[]
                            {
                                headerTopBox = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                                    Height = avatar_size,
                                    AutoSizeAxes = Axes.X,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Children = new[]
                                    {
                                        avatar = new UpdateableAvatar
                                        {
                                            Size = new Vector2(avatar_size),
                                            Masking = true,
                                            CornerRadius = avatar_size * 0.25f,
                                            OpenOnClick = { Value = false },
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            AutoSizeAxes = Axes.X,
                                            Padding = new MarginPadding { Left = 10 },
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Children = new Drawable[]
                                                    {
                                                        usernameText = new OsuSpriteText
                                                        {
                                                            Font = "Exo2.0-Regular",
                                                            TextSize = 24
                                                        },
                                                        openUserExternally = new ExternalLinkButton
                                                        {
                                                            Margin = new MarginPadding { Left = 5 },
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                        },
                                                    }
                                                },
                                                new FillFlowContainer
                                                {
                                                    Origin = Anchor.BottomLeft,
                                                    Anchor = Anchor.BottomLeft,
                                                    Direction = FillDirection.Vertical,
                                                    AutoSizeAxes = Axes.Both,
                                                    Children = new Drawable[]
                                                    {
                                                        titleText = new OsuSpriteText
                                                        {
                                                            TextSize = 18,
                                                            Font = "Exo2.0-Regular"
                                                        },
                                                        SupporterTag = new SupporterIcon
                                                        {
                                                            Height = 20,
                                                            Margin = new MarginPadding { Top = 5 }
                                                        },
                                                        userIconSeperatorBox = new Box
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Height = 1.5f,
                                                            Margin = new MarginPadding { Top = 10 }
                                                        },
                                                        new Container
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Margin = new MarginPadding { Top = 5 },
                                                            Children = new Drawable[]
                                                            {
                                                                userFlag = new DrawableFlag
                                                                {
                                                                    Size = new Vector2(30, 20)
                                                                },
                                                                userCountryText = new OsuSpriteText
                                                                {
                                                                    Font = "Exo2.0-Regular",
                                                                    TextSize = 17.5f,
                                                                    Margin = new MarginPadding { Left = 40 },
                                                                    Origin = Anchor.CentreLeft,
                                                                    Anchor = Anchor.CentreLeft,
                                                                }
                                                            }
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                userStats = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 300,
                                    Margin = new MarginPadding { Right = UserProfileOverlay.CONTENT_X_MARGIN },
                                    Padding = new MarginPadding { Vertical = 15 },
                                    Spacing = new Vector2(0, 2)
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                            Children = new Drawable[]
                            {
                                headerCenterBox = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding { Vertical = 10 },
                                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new ProfileHeaderButton
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Direction = FillDirection.Horizontal,
                                                    Padding = new MarginPadding { Right = 10 },
                                                    Children = new Drawable[]
                                                    {
                                                        new SpriteIcon
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Icon = FontAwesome.fa_user,
                                                            FillMode = FillMode.Fit,
                                                            Size = new Vector2(50, 14)
                                                        },
                                                        followerText = new OsuSpriteText
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            TextSize = 16,
                                                            Font = "Exo2.0-Bold"
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        messageButton = new ProfileHeaderButton
                                        {
                                            Alpha = 0,
                                            RelativeSizeAxes = Axes.Y,
                                            Children = new Drawable[]
                                            {
                                                new SpriteIcon
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Icon = FontAwesome.fa_envelope,
                                                    FillMode = FillMode.Fit,
                                                    Size = new Vector2(50, 14)
                                                },
                                            }
                                        },
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Vertical = 10 },
                                    Width = UserProfileOverlay.CONTENT_X_MARGIN,
                                    Child = expandButton = new ProfileHeaderButton
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Children = new Drawable[]
                                        {
                                            expandButtonIcon = new SpriteIcon
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(20),
                                                Icon = FontAwesome.fa_chevron_up,
                                            },
                                        }
                                    },
                                },
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Right = UserProfileOverlay.CONTENT_X_MARGIN },
                                    Children = new Drawable[]
                                    {
                                        new HasTooltipContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Size = new Vector2(40),
                                            TooltipText = "Level",
                                            Children = new Drawable[]
                                            {
                                                levelBadgeSprite = new Sprite
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                levelBadgeText = new OsuSpriteText
                                                {
                                                    TextSize = 20,
                                                    Font = "Exo2.0-Medium",
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                }
                                            }
                                        },
                                        expandedDetailContainer = new HasTooltipContainer
                                        {
                                            TooltipText = "Progress to next level",
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Width = 200,
                                            Height = 6,
                                            Margin = new MarginPadding { Right = 50 },
                                            Children = new Drawable[]
                                            {
                                                new CircularContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Masking = true,
                                                    Child = levelProgressBar = new Bar
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        BackgroundColour = Color4.Black,
                                                        Direction = BarDirection.LeftToRight,
                                                    }
                                                },
                                                levelProgressText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.TopRight,
                                                    Font = "Exo2.0-Bold",
                                                    TextSize = 12,
                                                }
                                            }
                                        },
                                        hiddenDetailContainer = new FillFlowContainer
                                        {
                                            Direction = FillDirection.Horizontal,
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Width = 200,
                                            AutoSizeAxes = Axes.Y,
                                            Alpha = 0,
                                            Spacing = new Vector2(10, 0),
                                            Margin = new MarginPadding { Right = 50 },
                                            Children = new[]
                                            {
                                                hiddenDetailGlobal = new OverlinedInfoContainer
                                                {
                                                    Title = "Global Ranking"
                                                },
                                                hiddenDetailCountry = new OverlinedInfoContainer
                                                {
                                                    Title = "Country Ranking"
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                headerDetailBox = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                headerDetailContainer = new FillFlowContainer
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
                                                        totalPlayTimeTooltip = new HasTooltipContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            TooltipText = "0 hours",
                                                            Child = totalPlayTimeInfo = new OverlinedInfoContainer
                                                            {
                                                                Title = "Total Play Time",
                                                            },
                                                        },
                                                        medalInfo = new OverlinedInfoContainer
                                                        {
                                                            Title = "Medals"
                                                        },
                                                        ppInfo = new OverlinedInfoContainer
                                                        {
                                                            Title = "pp"
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
                                                        detailGlobalRank = new OverlinedInfoContainer(true, 110)
                                                        {
                                                            Title = "Global Ranking"
                                                        },
                                                        detailCountryRank = new OverlinedInfoContainer(false, 110)
                                                        {
                                                            Title = "Country Ranking"
                                                        },
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        },
                        badgeContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                headerBadgeBox = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container //artificial shadow
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 3,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = new ColourInfo
                                        {
                                            TopLeft = Color4.Black.Opacity(0.2f),
                                            TopRight = Color4.Black.Opacity(0.2f),
                                            BottomLeft = Color4.Black.Opacity(0),
                                            BottomRight = Color4.Black.Opacity(0)
                                        }
                                    },
                                },
                                badgeFlowContainer = new FillFlowContainer
                                {
                                    Direction = FillDirection.Full,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Top = 5 },
                                    Spacing = new Vector2(10, 10),
                                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                headerBottomBox = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                                    Spacing = new Vector2(0, 10),
                                    Children = new Drawable[]
                                    {
                                        bottomTopLinkContainer = new LinkFlowContainer(text => text.TextSize = 12)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        },
                                        bottomLinkContainer = new LinkFlowContainer(text => text.TextSize = 12)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            infoTabControl.AddItem("Info");
            infoTabControl.AddItem("Modding");

            DetailsVisible.ValueChanged += newValue => expandButtonIcon.Icon = newValue ? FontAwesome.fa_chevron_down : FontAwesome.fa_chevron_up;
            DetailsVisible.ValueChanged += newValue => hiddenDetailContainer.Alpha = newValue ? 1 : 0;
            DetailsVisible.ValueChanged += newValue => expandedDetailContainer.Alpha = newValue ? 0 : 1;
            DetailsVisible.ValueChanged += newValue => headerDetailContainer.Alpha = newValue ? 0 : 1;
        }

        private Color4 communityUserGrayGreenLighter;

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, TextureStore textures)
        {
            coverInfoText.Colour = colours.CommunityUserGreen;

            infoTabControl.AccentColour = colours.CommunityUserGreen;

            headerTopBox.Colour = colours.CommunityUserGrayGreenDarker;
            userCountryText.Colour = colours.CommunityUserGrayGreenLighter;
            userIconSeperatorBox.Colour = colours.CommunityUserGrayGreenLighter;

            headerCenterBox.Colour = colours.CommunityUserGrayGreenDark;
            levelBadgeSprite.Texture = textures.Get("Profile/levelbadge");
            levelBadgeSprite.Colour = colours.Yellow;
            levelProgressBar.AccentColour = colours.Yellow;

            hiddenDetailGlobal.LineColour = colours.Yellow;
            hiddenDetailCountry.LineColour = colours.Yellow;

            headerDetailBox.Colour = colours.CommunityUserGrayGreenDarkest;
            totalPlayTimeInfo.LineColour = colours.Yellow;
            medalInfo.LineColour = colours.GreenLight;
            ppInfo.LineColour = colours.Red;

            detailGlobalRank.LineColour = colours.Yellow;
            detailCountryRank.LineColour = colours.Yellow;

            headerBadgeBox.Colour = colours.CommunityUserGrayGreenDarkest;
            headerBottomBox.Colour = colours.CommunityUserGrayGreenDarker;

            communityUserGrayGreenLighter = colours.CommunityUserGrayGreenLighter;
        }

        private User user;

        public User User
        {
            get => user;
            set
            {
                user = value;
                loadUser();
            }
        }

        private void loadUser()
        {
            LoadComponentAsync(new UserCoverBackground(user)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                OnLoadComplete = d => d.FadeInFromZero(200),
                Depth = float.MaxValue,
            }, coverContainer.Add);

            avatar.User = User;
            usernameText.Text = user.Username;
            openUserExternally.Link = $@"https://osu.ppy.sh/users/{user.Id}";
            userFlag.Country = user.Country;
            userCountryText.Text = user.Country?.FullName;
            SupporterTag.SupporterLevel = user.SupportLevel;
            if(user.Title != null)
                titleText.Text = user.Title;
            titleText.Colour = OsuColour.FromHex(user.Colour ?? "fff");

            userStats.Add(new UserStatsLine("Ranked Score", user.Statistics.RankedScore.ToString("#,##0")));
            userStats.Add(new UserStatsLine("Hit Accuracy", Math.Round(user.Statistics.Accuracy, 2).ToString("#0.00'%'")));
            userStats.Add(new UserStatsLine("Play Count", user.Statistics.PlayCount.ToString("#,##0")));
            userStats.Add(new UserStatsLine("Total Score", user.Statistics.TotalScore.ToString("#,##0")));
            userStats.Add(new UserStatsLine("Total Hits", user.Statistics.TotalHits.ToString("#,##0")));
            userStats.Add(new UserStatsLine("Maximum Combo", user.Statistics.MaxCombo.ToString("#,##0")));
            userStats.Add(new UserStatsLine("Replays Watched by Others", user.Statistics.ReplaysWatched.ToString("#,##0")));

            followerText.Text = user.FollowerCount?.Length > 0 ? user.FollowerCount[0].ToString("#,##0") : "0";

            if (!user.PMFriendsOnly && apiAccess.LocalUser.Value.Id != user.Id)
            {
                messageButton.Show();
                messageButton.Action = () =>
                {
                    channelManager?.OpenPrivateChannel(user);
                    userOverlay?.Hide();
                    chatOverlay?.Show();
                };
            }

            expandButton.Action = DetailsVisible.Toggle;

            levelBadgeText.Text = user.Statistics.Level.Current.ToString();
            levelProgressBar.Length = user.Statistics.Level.Progress / 100f;
            levelProgressText.Text = user.Statistics.Level.Progress.ToString("0'%'");

            hiddenDetailGlobal.Content = user.Statistics.Ranks.Global?.ToString("#,##0") ?? "-";
            hiddenDetailCountry.Content = user.Statistics.Ranks.Country?.ToString("#,##0") ?? "-";

            medalInfo.Content = user.Achievements.Length.ToString();
            ppInfo.Content = user.Statistics.PP?.ToString("#,##0") ?? "0";

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

            totalPlayTimeInfo.Content = formatTime(user.Statistics.PlayTime);
            totalPlayTimeTooltip.TooltipText = (user.Statistics.PlayTime ?? 0) / 3600 + " hours";

            foreach (var scoreRankInfo in scoreRankInfos)
                scoreRankInfo.Value.RankCount = user.Statistics.GradesCount.GetForScoreRank(scoreRankInfo.Key);

            detailGlobalRank.Content = user.Statistics.Ranks.Global?.ToString("#,##0") ?? "-";
            detailCountryRank.Content = user.Statistics.Ranks.Country?.ToString("#,##0") ?? "-";

            rankGraph.User.Value = user;

            var badges = User.Badges;
            if (badges.Length > 0)
            {
                badgeContainer.Show();
                for (var index = 0; index < badges.Length; index++)
                {
                    int displayIndex = index;
                    LoadComponentAsync(new DrawableBadge(badges[index]), asyncBadge =>
                    {
                        badgeFlowContainer.Add(asyncBadge);

                        // load in stable order regardless of async load order.
                        badgeFlowContainer.SetLayoutPosition(asyncBadge, displayIndex);
                    });
                }
            }

            void bold(SpriteText t) => t.Font = @"Exo2.0-Bold";
            void addSpacer(OsuTextFlowContainer textFlow) => textFlow.AddArbitraryDrawable(new Container { Width = 15 });

            if (user.JoinDate.ToUniversalTime().Year < 2008)
            {
                bottomTopLinkContainer.AddText("Here since the beginning");
            }
            else
            {
                bottomTopLinkContainer.AddText("Joined ");
                bottomTopLinkContainer.AddText(new DrawableDate(user.JoinDate), bold);
            }

            addSpacer(bottomTopLinkContainer);

            if (user.PlayStyle?.Length > 0)
            {
                bottomTopLinkContainer.AddText("Plays with ");
                bottomTopLinkContainer.AddText(string.Join(", ", user.PlayStyle.Select(style => play_styles[style])), bold);

                addSpacer(bottomTopLinkContainer);
            }

            if (user.LastVisit.HasValue)
            {
                bottomTopLinkContainer.AddText("Last seen ");
                bottomTopLinkContainer.AddText(new DrawableDate(user.LastVisit.Value), bold);

                addSpacer(bottomTopLinkContainer);
            }

            bottomTopLinkContainer.AddText("Contributed ");
            bottomTopLinkContainer.AddLink($@"{user.PostCount:#,##0} forum posts", $"https://osu.ppy.sh/users/{user.Id}/posts", creationParameters: bold);

            void tryAddInfo(FontAwesome icon, string content, string link = null)
            {
                if (string.IsNullOrEmpty(content)) return;

                bottomLinkContainer.AddIcon(icon, text =>
                {
                    text.TextSize = 10;
                    text.Colour = communityUserGrayGreenLighter;
                });
                if (link != null)
                {
                    bottomLinkContainer.AddLink(" " + content, link, creationParameters: bold);
                }
                else
                {
                    bottomLinkContainer.AddText(" " + content, bold);
                }
                addSpacer(bottomLinkContainer);
            }

            string websiteWithoutProtcol = user.Website;
            if (!string.IsNullOrEmpty(websiteWithoutProtcol))
            {
                int protocolIndex = websiteWithoutProtcol.IndexOf("//", StringComparison.Ordinal);
                if (protocolIndex >= 0)
                    websiteWithoutProtcol = websiteWithoutProtcol.Substring(protocolIndex + 2);
            }

            tryAddInfo(FontAwesome.fa_map_marker, user.Location);
            tryAddInfo(FontAwesome.fa_heart_o, user.Interests);
            tryAddInfo(FontAwesome.fa_suitcase, user.Occupation);
            bottomLinkContainer.NewLine();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfo(FontAwesome.fa_twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfo(FontAwesome.fa_gamepad, user.Discord); //todo: update fontawesome to include discord logo
            tryAddInfo(FontAwesome.fa_skype, user.Skype, @"skype:" + user.Skype + @"?chat");
            tryAddInfo(FontAwesome.fa_lastfm, user.Lastfm, $@"https://last.fm/users/{user.Lastfm}");
            tryAddInfo(FontAwesome.fa_link, websiteWithoutProtcol, user.Website);
        }

        private class UserStatsLine : Container
        {
            private readonly OsuSpriteText rightText;

            public UserStatsLine(string left, string right)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        TextSize = 15,
                        Text = left,
                        Font = "Exo2.0-Medium"
                    },
                    rightText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        TextSize = 15,
                        Text = right,
                        Font = "Exo2.0-Medium"
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                rightText.Colour = colours.BlueLight;
            }
        }

        private class ProfileHeaderButton : OsuHoverContainer
        {
            private readonly Box background;
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public ProfileHeaderButton()
            {
                HoverColour = Color4.Black.Opacity(0.75f);
                IdleColour = Color4.Black.Opacity(0.7f);
                AutoSizeAxes = Axes.X;

                base.Content.Add(new CircularContainer
                {
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10 },
                        }
                    }
                });
            }
        }

        private class HasTooltipContainer : Container, IHasTooltip
        {
            public string TooltipText { get; set; }
        }

        private class OverlinedInfoContainer : CompositeDrawable
        {
            private readonly Circle line;
            private readonly OsuSpriteText title, content;

            public string Title
            {
                set => title.Text = value;
            }

            public string Content
            {
                set => content.Text = value;
            }

            public Color4 LineColour
            {
                set => line.Colour = value;
            }

            public OverlinedInfoContainer(bool big = false, int minimumWidth = 60)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        line = new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 4,
                        },
                        title = new OsuSpriteText
                        {
                            Font = "Exo2.0-Bold",
                            TextSize = big ? 14 : 12,
                        },
                        content = new OsuSpriteText
                        {
                            Font = "Exo2.0-Light",
                            TextSize = big ? 40 : 18,
                        },
                        new Container //Add a minimum size to the FillFlowContainer
                        {
                            Width = minimumWidth,
                        }
                    }
                };
            }
        }

        public class ScoreRankInfo : CompositeDrawable
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

        private class DrawableBadge : CompositeDrawable, IHasTooltip
        {
            public static readonly Vector2 DRAWABLE_BADGE_SIZE = new Vector2(86, 40);

            private readonly Badge badge;

            public DrawableBadge(Badge badge)
            {
                this.badge = badge;
                Size = DRAWABLE_BADGE_SIZE;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                InternalChild = new Sprite
                {
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get(badge.ImageUrl),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                InternalChild.FadeInFromZero(200);
            }

            public string TooltipText => badge.Description;
        }
    }
}
