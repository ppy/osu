// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class TopHeaderContainer : CompositeDrawable
    {
        private const float avatar_size = 110;

        public readonly Bindable<User> User = new Bindable<User>();

        private SupporterIcon supporterTag;
        private UpdateableAvatar avatar;
        private OsuSpriteText usernameText;
        private ExternalLinkButton openUserExternally;
        private OsuSpriteText titleText;
        private UpdateableFlag userFlag;
        private OsuSpriteText userCountryText;
        private FillFlowContainer userStats;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = 150;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Height = avatar_size,
                    AutoSizeAxes = Axes.X,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        avatar = new UpdateableAvatar
                        {
                            Size = new Vector2(avatar_size),
                            Masking = true,
                            CornerRadius = avatar_size * 0.25f,
                            OpenOnClick = { Value = false },
                            ShowGuestOnNull = false,
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
                                    Direction = FillDirection.Vertical,
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
                                                    Font = OsuFont.GetFont(size: 24, weight: FontWeight.Regular)
                                                },
                                                openUserExternally = new ExternalLinkButton
                                                {
                                                    Margin = new MarginPadding { Left = 5 },
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                            }
                                        },
                                        titleText = new OsuSpriteText
                                        {
                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Regular)
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
                                        supporterTag = new SupporterIcon
                                        {
                                            Height = 20,
                                            Margin = new MarginPadding { Top = 5 }
                                        },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 1.5f,
                                            Margin = new MarginPadding { Top = 10 },
                                            Colour = colourProvider.Light1,
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Margin = new MarginPadding { Top = 5 },
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                userFlag = new UpdateableFlag
                                                {
                                                    Size = new Vector2(30, 20),
                                                    ShowPlaceholderOnNull = false,
                                                },
                                                userCountryText = new OsuSpriteText
                                                {
                                                    Font = OsuFont.GetFont(size: 17.5f, weight: FontWeight.Regular),
                                                    Margin = new MarginPadding { Left = 10 },
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    Colour = colourProvider.Light1,
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

            User.BindValueChanged(user => updateUser(user.NewValue));
        }

        private void updateUser(User user)
        {
            avatar.User = user;
            usernameText.Text = user?.Username ?? string.Empty;
            openUserExternally.Link = $@"https://osu.ppy.sh/users/{user?.Id ?? 0}";
            userFlag.Country = user?.Country;
            userCountryText.Text = user?.Country?.FullName ?? "Alien";
            supporterTag.SupportLevel = user?.SupportLevel ?? 0;
            titleText.Text = user?.Title ?? string.Empty;
            titleText.Colour = OsuColour.FromHex(user?.Colour ?? "fff");

            userStats.Clear();

            if (user?.Statistics != null)
            {
                userStats.Add(new UserStatsLine("Ranked Score", user.Statistics.RankedScore.ToString("#,##0")));
                userStats.Add(new UserStatsLine("Hit Accuracy", user.Statistics.DisplayAccuracy));
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
                        Font = OsuFont.GetFont(size: 15),
                        Text = left,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                        Text = right,
                    },
                };
            }
        }
    }
}
