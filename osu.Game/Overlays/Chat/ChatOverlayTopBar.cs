// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatOverlayTopBar : Container
    {
        private Box background = null!;

        private Color4 backgroundColour;

        public Drawable DragBar = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = backgroundColour = colourProvider.Background3,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 50),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Sprite
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = textures.Get("Icons/Hexacons/messaging"),
                                Size = new Vector2(24),
                            },
                            // Placeholder text
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = ChatStrings.TitleCompact,
                                Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Bottom = 2f },
                            },
                        },
                    },
                },
                DragBar = new DragArea
                {
                    Alpha = RuntimeInfo.IsMobile ? 1 : 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colourProvider.Background4,
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!RuntimeInfo.IsMobile)
                DragBar.FadeIn(100);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!RuntimeInfo.IsMobile)
                DragBar.FadeOut(100);
            base.OnHoverLost(e);
        }

        private partial class DragArea : CompositeDrawable
        {
            private readonly Circle circle;

            public DragArea()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(150, 7),
                        Margin = new MarginPadding(12),
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateScale();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateScale();
                base.OnHoverLost(e);
            }

            private bool dragging;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                dragging = true;
                updateScale();
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                dragging = false;
                updateScale();
                base.OnMouseUp(e);
            }

            private void updateScale()
            {
                if (dragging || IsHovered)
                    circle.FadeIn(100);
                else
                    circle.FadeTo(0.6f, 100);

                if (dragging)
                    circle.ScaleTo(1f, 400, Easing.OutQuint);
                else if (IsHovered)
                    circle.ScaleTo(1.05f, 400, Easing.OutElasticHalf);
                else
                    circle.ScaleTo(1f, 500, Easing.OutQuint);
            }
        }
    }
}
