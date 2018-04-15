// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using OpenTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class BadgeContainer : Container
    {
        private const float outer_container_width = 98;
        private const float outer_container_padding = 3;
        private static readonly Vector2 badge_size = new Vector2(outer_container_width - outer_container_padding * 2, 46);

        private OsuSpriteText badgeCountText;
        private FillFlowContainer badgeFlowContainer;
        private FillFlowContainer outerBadgeContainer;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new Container
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Masking = true,
                CornerRadius = 4,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray3
                    },
                    outerBadgeContainer = new OuterBadgeContainer(onOuterHover, onOuterHoverLost)
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(outer_container_padding),
                        Width = outer_container_width,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            badgeCountText = new OsuSpriteText
                            {
                                Alpha = 0,
                                TextSize = 12,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Font = "Exo2.0-Regular"
                            },
                            new Container
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                AutoSizeAxes = Axes.Both,
                                Child = badgeFlowContainer = new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    AutoSizeAxes = Axes.Both,
                                }
                            }
                        }
                    },
                }
            };

            Scheduler.AddDelayed(rotateBadges, 3000, true);
        }

        private void rotateBadges()
        {
            if (outerBadgeContainer.IsHovered) return;

            visibleBadge = (visibleBadge + 1) % badgeCount;

            badgeFlowContainer.MoveToX(-badge_size.X * visibleBadge, 500, Easing.InOutQuad);
        }

        private int visibleBadge;
        private int badgeCount;

        public void ShowBadges(Badge[] badges)
        {
            switch (badges.Length)
            {
                case 0:
                    Hide();
                    return;
                case 1:
                    badgeCountText.Hide();
                    break;
                default:
                    badgeCountText.Show();
                    badgeCountText.Text = $"{badges.Length} badges";
                    break;
            }

            Show();
            badgeCount = badges.Length;
            visibleBadge = 0;

            foreach (var badge in badges)
            {
                LoadComponentAsync(new DrawableBadge(badge)
                {
                    Size = badge_size,
                }, badgeFlowContainer.Add);
            }
        }

        private void onOuterHover()
        {
            badgeFlowContainer.ClearTransforms();
            badgeFlowContainer.X = 0;
            badgeFlowContainer.Direction = FillDirection.Full;
            outerBadgeContainer.AutoSizeAxes = Axes.Both;

            badgeFlowContainer.MaximumSize = new Vector2(ChildSize.X, float.MaxValue);
        }

        private void onOuterHoverLost()
        {
            rotateBadges();
            badgeFlowContainer.Direction = FillDirection.Horizontal;
            outerBadgeContainer.AutoSizeAxes = Axes.Y;
            outerBadgeContainer.Width = outer_container_width;
        }

        private class OuterBadgeContainer : FillFlowContainer
        {
            private readonly Action hoverAction;
            private readonly Action hoverLostAction;

            public OuterBadgeContainer(Action hoverAction, Action hoverLostAction)
            {
                this.hoverAction = hoverAction;
                this.hoverLostAction = hoverLostAction;
            }

            protected override bool OnHover(InputState state)
            {
                hoverAction();
                return true;
            }

            protected override void OnHoverLost(InputState state) => hoverLostAction();
        }

        private class DrawableBadge : Container, IHasTooltip
        {
            private readonly Badge badge;

            public DrawableBadge(Badge badge)
            {
                this.badge = badge;
                Padding = new MarginPadding(3);
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Child = new Sprite
                {
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get(badge.ImageUrl),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    OnLoadComplete = d => d.FadeInFromZero(200)
                };
            }

            public string TooltipText => badge.Description;
        }
    }
}
