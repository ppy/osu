// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class CentreHeaderContainer : CompositeDrawable
    {
        public readonly BindableBool DetailsVisible = new BindableBool(true);
        public readonly Bindable<User> User = new Bindable<User>();

        private OsuSpriteText followerText;
        private OverlinedInfoContainer hiddenDetailGlobal;
        private OverlinedInfoContainer hiddenDetailCountry;

        public CentreHeaderContainer()
        {
            Height = 60;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            Container<Drawable> hiddenDetailContainer;
            Container<Drawable> expandedDetailContainer;
            SpriteIcon expandButtonIcon;

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
                    Child = new ExpandButton
                    {
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => DetailsVisible.Toggle(),
                        Children = new Drawable[]
                        {
                            expandButtonIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(20, 12),
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
                        new LevelBadge
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(40),
                            User = { BindTarget = User }
                        },
                        expandedDetailContainer = new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Width = 200,
                            Height = 6,
                            Margin = new MarginPadding { Right = 50 },
                            Child = new LevelProgressBar
                            {
                                RelativeSizeAxes = Axes.Both,
                                User = { BindTarget = User }
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

            DetailsVisible.BindValueChanged(visible =>
            {
                expandButtonIcon.Icon = visible.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
                hiddenDetailContainer.Alpha = visible.NewValue ? 1 : 0;
                expandedDetailContainer.Alpha = visible.NewValue ? 0 : 1;
            }, true);

            User.BindValueChanged(user => updateDisplay(user.NewValue));
        }

        private void updateDisplay(User user)
        {
            followerText.Text = user?.FollowerCount?.Length > 0 ? user.FollowerCount[0].ToString("#,##0") : "0";

            hiddenDetailGlobal.Content = user?.Statistics?.Ranks.Global?.ToString("#,##0") ?? "-";
            hiddenDetailCountry.Content = user?.Statistics?.Ranks.Country?.ToString("#,##0") ?? "-";
        }

        private class ExpandButton : ProfileHeaderButton
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.CommunityUserGrayGreen;
                HoverColour = colours.CommunityUserGrayGreen.Darken(0.2f);
            }
        }
    }
}
