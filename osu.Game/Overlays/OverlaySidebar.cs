// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public abstract class OverlaySidebar : CompositeDrawable
    {
        private readonly Box sidebarBackground;
        private readonly Box scrollbarBackground;

        protected OverlaySidebar()
        {
            RelativeSizeAxes = Axes.Y;
            Width = 250;
            InternalChildren = new Drawable[]
            {
                sidebarBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                scrollbarBackground = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = OsuScrollContainer.SCROLL_BAR_HEIGHT,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = 0.5f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = -3 }, // Compensate for scrollbar margin
                    Child = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 3 }, // Addeded 3px back
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Vertical = 20,
                                    Left = 50,
                                    Right = 30
                                },
                                Child = CreateContent()
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            sidebarBackground.Colour = colourProvider.Background4;
            scrollbarBackground.Colour = colourProvider.Background3;
        }

        [NotNull]
        protected virtual Drawable CreateContent() => Empty();
    }
}
