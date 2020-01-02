// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayHeader : Container
    {
        protected readonly OverlayHeaderTabControl TabControl;

        private readonly Box titleBackground;
        private readonly Box controlBackground;
        private readonly Container background;

        protected Color4 TitleBackgroundColour
        {
            set => titleBackground.Colour = value;
        }

        protected Color4 ControlBackgroundColour
        {
            set => controlBackground.Colour = value;
        }

        protected float BackgroundHeight
        {
            set => background.Height = value;
        }

        protected OverlayHeader()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    background = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 80,
                        Masking = true,
                        Child = CreateBackground()
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            titleBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray,
                            },
                            CreateTitle().With(title =>
                            {
                                title.Margin = new MarginPadding
                                {
                                    Vertical = 10,
                                    Left = UserProfileOverlay.CONTENT_X_MARGIN
                                };
                            })
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Depth = -float.MaxValue,
                        Children = new Drawable[]
                        {
                            controlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray,
                            },
                            TabControl = new OverlayHeaderTabControl
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 30,
                                Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                            }
                        }
                    },
                    CreateContent()
                }
            });
        }

        protected abstract Drawable CreateBackground();

        [NotNull]
        protected virtual Drawable CreateContent() => new Container();

        protected abstract ScreenTitle CreateTitle();
    }
}
