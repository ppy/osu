// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class CenterHeaderContainer : Container
    {
        public readonly BindableBool DetailsVisible = new BindableBool();

        private OsuSpriteText followerText;
        private ProfileHeaderButton messageButton;
        private OsuSpriteText levelBadgeText;

        private Bar levelProgressBar;
        private OsuSpriteText levelProgressText;

        private ProfileHeader.OverlinedInfoContainer hiddenDetailGlobal, hiddenDetailCountry;

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay userOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private APIAccess apiAccess { get; set; }

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
        private void load(OsuColour colours, TextureStore textures)
        {
            Container<Drawable> hiddenDetailContainer, expandedDetailContainer;
            SpriteIcon expandButtonIcon;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.CommunityUserGrayGreenDark
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
                    Child = new ProfileHeaderButton
                    {
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = DetailsVisible.Toggle,
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
                        new ProfileHeader.HasTooltipContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(40),
                            TooltipText = "Level",
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Texture = textures.Get("Profile/levelbadge"),
                                    Colour = colours.Yellow,
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
                        expandedDetailContainer = new ProfileHeader.HasTooltipContainer
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
                                        AccentColour = colours.Yellow
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
                                hiddenDetailGlobal = new ProfileHeader.OverlinedInfoContainer
                                {
                                    Title = "Global Ranking",
                                    LineColour = colours.Yellow
                                },
                                hiddenDetailCountry = new ProfileHeader.OverlinedInfoContainer
                                {
                                    Title = "Country Ranking",
                                    LineColour = colours.Yellow
                                },
                            }
                        }
                    }
                }
            };

            DetailsVisible.ValueChanged += newValue => expandButtonIcon.Icon = newValue ? FontAwesome.fa_chevron_down : FontAwesome.fa_chevron_up;
            DetailsVisible.ValueChanged += newValue => hiddenDetailContainer.Alpha = newValue ? 1 : 0;
            DetailsVisible.ValueChanged += newValue => expandedDetailContainer.Alpha = newValue ? 0 : 1;
        }

        private void updateDisplay()
        {
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
            else
            {
                messageButton.Hide();
            }

            levelBadgeText.Text = user.Statistics?.Level.Current.ToString() ?? "0";
            levelProgressBar.Length = user.Statistics?.Level.Progress / 100f ?? 0;
            levelProgressText.Text = user.Statistics?.Level.Progress.ToString("0'%'");

            hiddenDetailGlobal.Content = user?.Statistics?.Ranks.Global?.ToString("#,##0") ?? "-";
            hiddenDetailCountry.Content = user?.Statistics?.Ranks.Country?.ToString("#,##0") ?? "-";

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
    }
}
