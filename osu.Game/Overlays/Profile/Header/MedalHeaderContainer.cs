// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class MedalHeaderContainer : Container
    {
        private FillFlowContainer badgeFlowContainer;

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
            Alpha = 0;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.CommunityUserGrayGreenDarkest,
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
            };
        }

        private void updateDisplay()
        {
            var badges = User.Badges;
            badgeFlowContainer.Clear();
            if (badges?.Length > 0)
            {
                Show();
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
            else
            {
                Hide();
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
