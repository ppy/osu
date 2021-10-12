// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Graphics.Containers
{
    public class OsuScrollContainer : OsuScrollContainer<Drawable>
    {
        public OsuScrollContainer()
        {
        }

        public OsuScrollContainer(Direction direction)
            : base(direction)
        {
        }
    }

    public class OsuScrollContainer<T> : ScrollContainer<T>, IHasAccentColour
        where T : Drawable
    {
        public const float SCROLL_BAR_HEIGHT = 10;
        public const float SCROLL_BAR_PADDING = 3;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;

                if (Scrollbar is IHasAccentColour accentedScrollbar)
                    accentedScrollbar.AccentColour = value;
            }
        }

        /// <summary>
        /// Allows controlling the scroll bar from any position in the container using the right mouse button.
        /// Uses the value of <see cref="DistanceDecayOnRightMouseScrollbar"/> to smoothly scroll to the dragged location.
        /// </summary>
        public bool RightMouseScrollbar;

        /// <summary>
        /// Controls the rate with which the target position is approached when performing a relative drag. Default is 0.02.
        /// </summary>
        public double DistanceDecayOnRightMouseScrollbar = 0.02;

        private bool shouldPerformRightMouseScroll(MouseButtonEvent e) => RightMouseScrollbar && e.Button == MouseButton.Right;

        private void scrollFromMouseEvent(MouseEvent e) =>
            ScrollTo(Clamp(ToLocalSpace(e.ScreenSpaceMousePosition)[ScrollDim] / DrawSize[ScrollDim]) * Content.DrawSize[ScrollDim], true, DistanceDecayOnRightMouseScrollbar);

        private bool rightMouseDragging;

        protected override bool IsDragging => base.IsDragging || rightMouseDragging;

        public OsuScrollContainer(Direction scrollDirection = Direction.Vertical)
            : base(scrollDirection)
        {
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (shouldPerformRightMouseScroll(e))
            {
                scrollFromMouseEvent(e);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (rightMouseDragging)
            {
                scrollFromMouseEvent(e);
                return;
            }

            base.OnDrag(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (shouldPerformRightMouseScroll(e))
            {
                rightMouseDragging = true;
                return true;
            }

            return base.OnDragStart(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (rightMouseDragging)
            {
                rightMouseDragging = false;
                return;
            }

            base.OnDragEnd(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // allow for controlling volume when alt is held.
            // mostly for compatibility with osu-stable.
            if (e.AltPressed) return false;

            return base.OnScroll(e);
        }

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new OsuScrollbar(direction);

        protected class OsuScrollbar : ScrollbarContainer, IHasAccentColour
        {
            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;

                    if (IsLoaded)
                        updateMouseDownState();
                }
            }

            private Color4 hoverColour;
            private Color4 defaultColour;

            private bool mouseDown;
            private const int fade_duration = 100;

            private readonly Box box;

            public OsuScrollbar(Direction scrollDir)
                : base(scrollDir)
            {
                Blending = BlendingParameters.Additive;

                CornerRadius = 5;

                // needs to be set initially for the ResizeTo to respect minimum size
                Size = new Vector2(SCROLL_BAR_HEIGHT);

                const float margin = 3;

                Margin = new MarginPadding
                {
                    Left = scrollDir == Direction.Vertical ? margin : 0,
                    Right = scrollDir == Direction.Vertical ? margin : 0,
                    Top = scrollDir == Direction.Horizontal ? margin : 0,
                    Bottom = scrollDir == Direction.Horizontal ? margin : 0,
                };

                Masking = true;
                Child = box = new Box { RelativeSizeAxes = Axes.Both };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = defaultColour = colours.Gray8;
                hoverColour = colours.GrayF;
                AccentColour = colours.Green;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateHoverState();
                updateMouseDownState();
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                Vector2 size = new Vector2(SCROLL_BAR_HEIGHT)
                {
                    [(int)ScrollDirection] = val
                };
                this.ResizeTo(size, duration, easing);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateHoverState(fade_duration);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateHoverState(fade_duration);
            }

            private void updateHoverState(double duration = 0) => this.FadeColour(IsHovered ? hoverColour : defaultColour, duration);

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!base.OnMouseDown(e)) return false;

                mouseDown = true;
                updateMouseDownState(fade_duration);

                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button != MouseButton.Left) return;

                mouseDown = false;
                updateMouseDownState(fade_duration);

                base.OnMouseUp(e);
            }

            private void updateMouseDownState(double duration = 0)
            {
                // note that we are changing the colour of the box here as to not interfere with the hover effect.
                box.FadeColour(mouseDown ? AccentColour : Color4.White, duration);
            }
        }
    }
}
