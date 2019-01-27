// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class TopHeaderContainer : Container
    {
        public SupporterIcon SupporterTag;
        private UpdateableAvatar avatar;
        private OsuSpriteText usernameText;
        private ExternalLinkButton openUserExternally;
        private OsuSpriteText titleText;
        private DrawableFlag userFlag;
        private OsuSpriteText userCountryText;
        private FillFlowContainer userStats;

        private const float avatar_size = 110;

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
                    Colour = colours.CommunityUserGrayGreenDarker,
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
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 1.5f,
                                            Margin = new MarginPadding { Top = 10 },
                                            Colour = colours.CommunityUserGrayGreenLighter,
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
                                                    Colour = colours.CommunityUserGrayGreenLighter,
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
            };
        }

        private void updateDisplay()
        {
            avatar.User = User;
            usernameText.Text = user.Username;
            openUserExternally.Link = $@"https://osu.ppy.sh/users/{user.Id}";
            userFlag.Country = user.Country;
            userCountryText.Text = user.Country?.FullName ?? "Alien";
            SupporterTag.SupporterLevel = user.SupportLevel;
            titleText.Text = user.Title;
            titleText.Colour = OsuColour.FromHex(user.Colour ?? "fff");

            userStats.Clear();
            if (user.Statistics != null)
            {
                userStats.Add(new UserStatsLine("Ranked Score", user.Statistics.RankedScore.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Hit Accuracy", Math.Round(user.Statistics.Accuracy, 2).ToString("#0.00'%'")));
                userStats.Add(new UserStatsLine("Play Count", user.Statistics.PlayCount.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Total Score", user.Statistics.TotalScore.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Total Hits", user.Statistics.TotalHits.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Maximum Combo", user.Statistics.MaxCombo.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Replays Watched by Others", user.Statistics.ReplaysWatched.ToString("#,##0")));
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
                        TextSize = 15,
                        Text = left,
                        Font = "Exo2.0-Medium"
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        TextSize = 15,
                        Text = right,
                        Font = "Exo2.0-Bold"
                    },
                };
            }
        }
    }
}
