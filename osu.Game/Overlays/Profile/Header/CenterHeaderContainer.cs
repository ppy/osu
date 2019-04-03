// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class CenterHeaderContainer : CompositeDrawable
    {
        public Action<bool> DetailsVisibilityAction;
        private bool detailsVisible;

        private OsuSpriteText followerText;
        private OsuSpriteText levelBadgeText;

        private Bar levelProgressBar;
        private OsuSpriteText levelProgressText;

        private OverlinedInfoContainer hiddenDetailGlobal, hiddenDetailCountry;

        public readonly Bindable<User> User = new Bindable<User>();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            Container<Drawable> hiddenDetailContainer, expandedDetailContainer;
            SpriteIcon expandButtonIcon;
            ProfileHeaderButton detailsToggleButton;
            Height = 60;
            User.ValueChanged += e => updateDisplay(e.NewValue);

            InternalChildren = new Drawable[]
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
                                            Icon = FontAwesome.Solid.User,
                                            FillMode = FillMode.Fit,
                                            Size = new Vector2(50, 14)
                                        },
                                        followerText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(weight: FontWeight.Bold)
                                        }
                                    }
                                }
                            }
                        },
                        new ProfileMessageButton
                        {
                            User = { BindTarget = User }
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
                    Child = detailsToggleButton = new ProfileHeaderButton
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
                                Icon = FontAwesome.Solid.ChevronUp,
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
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 20)
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
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold)
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
                                    Title = "Global Ranking",
                                    LineColour = colours.Yellow
                                },
                                hiddenDetailCountry = new OverlinedInfoContainer
                                {
                                    Title = "Country Ranking",
                                    LineColour = colours.Yellow
                                },
                            }
                        }
                    }
                }
            };

            detailsToggleButton.Action = () =>
            {
                detailsVisible = !detailsVisible;
                expandButtonIcon.Icon = detailsVisible ? FontAwesome.Solid.ChevronDown : FontAwesome.Solid.ChevronUp;
                hiddenDetailContainer.Alpha = detailsVisible ? 1 : 0;
                expandedDetailContainer.Alpha = detailsVisible ? 0 : 1;
                DetailsVisibilityAction(detailsVisible);
            };
        }

        private void updateDisplay(User user)
        {
            followerText.Text = user.FollowerCount?.Length > 0 ? user.FollowerCount[0].ToString("#,##0") : "0";

            levelBadgeText.Text = user.Statistics?.Level.Current.ToString() ?? "0";
            levelProgressBar.Length = user.Statistics?.Level.Progress / 100f ?? 0;
            levelProgressText.Text = user.Statistics?.Level.Progress.ToString("0'%'");

            hiddenDetailGlobal.Content = user.Statistics?.Ranks.Global?.ToString("#,##0") ?? "-";
            hiddenDetailCountry.Content = user.Statistics?.Ranks.Country?.ToString("#,##0") ?? "-";
        }
    }
}
