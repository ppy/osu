// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public abstract partial class OverlaySidebar : CompositeDrawable
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
                    Width = OsuScrollContainer.SCROLL_BAR_WIDTH,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = 0.5f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = -3 }, // Compensate for scrollbar margin
                    Child = new SidebarScrollContainer
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
                                    Left = WaveOverlayContainer.HORIZONTAL_PADDING,
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

        protected virtual Drawable CreateContent() => Empty();

        private partial class SidebarScrollContainer : OsuScrollContainer
        {
            protected override bool OnScroll(ScrollEvent e)
            {
                if (e.ScrollDelta.Y > 0 && IsScrolledToStart())
                    return false;

                if (e.ScrollDelta.Y < 0 && IsScrolledToEnd())
                    return false;

                return base.OnScroll(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                if (e.Delta.Y > 0 && IsScrolledToStart())
                    return false;

                if (e.Delta.Y < 0 && IsScrolledToEnd())
                    return false;

                return base.OnDragStart(e);
            }
        }
    }
}
