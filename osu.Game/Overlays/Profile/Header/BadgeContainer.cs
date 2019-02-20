// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class BadgeContainer : Container
    {
        private static readonly Vector2 badge_size = new Vector2(86, 40);
        private static readonly MarginPadding outer_padding = new MarginPadding(3);

        private OsuSpriteText badgeCountText;
        private FillFlowContainer badgeFlowContainer;
        private FillFlowContainer outerBadgeContainer;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new Container
            {
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
                        Padding = outer_padding,
                        Width = DrawableBadge.DRAWABLE_BADGE_SIZE.X + outer_padding.TotalHorizontal,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            badgeCountText = new OsuSpriteText
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Alpha = 0,
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular)
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

            badgeFlowContainer.MoveToX(-DrawableBadge.DRAWABLE_BADGE_SIZE.X * visibleBadge, 500, Easing.InOutQuad);
        }

        private int visibleBadge;
        private int badgeCount;

        public void ShowBadges(Badge[] badges)
        {
            if (badges == null || badges.Length == 0)
            {
                Hide();
                return;
            }

            badgeCount = badges.Length;

            badgeCountText.FadeTo(badgeCount > 1 ? 1 : 0);
            badgeCountText.Text = $"{badges.Length} badges";

            Show();
            visibleBadge = 0;

            badgeFlowContainer.Clear();
            for (var index = 0; index < badges.Length; index++)
            {
                int displayIndex = index;
                LoadComponentAsync(new DrawableBadge(badges[index])
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }, asyncBadge =>
                {
                    badgeFlowContainer.Add(asyncBadge);

                    // load in stable order regardless of async load order.
                    badgeFlowContainer.SetLayoutPosition(asyncBadge, displayIndex);
                });
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
            badgeFlowContainer.X = -DrawableBadge.DRAWABLE_BADGE_SIZE.X * visibleBadge;
            badgeFlowContainer.Direction = FillDirection.Horizontal;
            outerBadgeContainer.AutoSizeAxes = Axes.Y;
            outerBadgeContainer.Width = DrawableBadge.DRAWABLE_BADGE_SIZE.X + outer_padding.TotalHorizontal;
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

            protected override bool OnHover(HoverEvent e)
            {
                hoverAction();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e) => hoverLostAction();
        }

        private class DrawableBadge : Container, IHasTooltip
        {
            public static readonly Vector2 DRAWABLE_BADGE_SIZE = badge_size + outer_padding.Total;

            private readonly Badge badge;

            public DrawableBadge(Badge badge)
            {
                this.badge = badge;
                Padding = outer_padding;
                Size = DRAWABLE_BADGE_SIZE;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                Child = new Sprite
                {
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get(badge.ImageUrl),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Child.FadeInFromZero(200);
            }

            public string TooltipText => badge.Description;
        }
    }
}
