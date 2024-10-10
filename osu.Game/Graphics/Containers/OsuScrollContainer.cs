// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Graphics.Containers
{
    public partial class OsuScrollContainer : OsuScrollContainer<Drawable>
    {
        public OsuScrollContainer()
        {
        }

        public OsuScrollContainer(Direction direction)
            : base(direction)
        {
        }
    }

    public partial class OsuScrollContainer<T> : ScrollContainer<T> where T : Drawable
    {
        public const float SCROLL_BAR_WIDTH = 10;
        public const float SCROLL_BAR_PADDING = 3;

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

        private bool rightMouseDragging;

        protected override bool IsDragging => base.IsDragging || rightMouseDragging;

        public OsuScrollContainer(Direction scrollDirection = Direction.Vertical)
            : base(scrollDirection)
        {
        }

        /// <summary>
        /// Scrolls a <see cref="Drawable"/> into view.
        /// </summary>
        /// <param name="d">The <see cref="Drawable"/> to scroll into view.</param>
        /// <param name="animated">Whether to animate the movement.</param>
        /// <param name="extraScroll">An added amount to scroll beyond the requirement to bring the target into view.</param>
        public void ScrollIntoView(Drawable d, bool animated = true, float extraScroll = 0)
        {
            float childPos0 = GetChildPosInContent(d);
            float childPos1 = GetChildPosInContent(d, d.DrawSize);

            float minPos = Math.Min(childPos0, childPos1);
            float maxPos = Math.Max(childPos0, childPos1);

            if (minPos < Current || (minPos > Current && d.DrawSize[ScrollDim] > DisplayableContent))
                ScrollTo(minPos - extraScroll, animated);
            else if (maxPos > Current + DisplayableContent)
                ScrollTo(maxPos - DisplayableContent + extraScroll, animated);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (shouldPerformRightMouseScroll(e))
            {
                ScrollFromMouseEvent(e);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (rightMouseDragging)
            {
                ScrollFromMouseEvent(e);
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

        protected virtual void ScrollFromMouseEvent(MouseEvent e) =>
            ScrollTo(Clamp(ToLocalSpace(e.ScreenSpaceMousePosition)[ScrollDim] / DrawSize[ScrollDim] * Content.DrawSize[ScrollDim]), true, DistanceDecayOnRightMouseScrollbar);

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new OsuScrollbar(direction);

        protected partial class OsuScrollbar : ScrollbarContainer
        {
            private Color4 hoverColour;
            private Color4 defaultColour;
            private Color4 highlightColour;

            private readonly Box box;

            protected override float MinimumDimSize => SCROLL_BAR_WIDTH * 3;

            public OsuScrollbar(Direction scrollDir)
                : base(scrollDir)
            {
                Blending = BlendingParameters.Additive;

                CornerRadius = 5;

                // needs to be set initially for the ResizeTo to respect minimum size
                Size = new Vector2(SCROLL_BAR_WIDTH);

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

            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider, OsuColour colours)
            {
                Colour = defaultColour = colours.Gray8;
                hoverColour = colours.GrayF;
                highlightColour = colourProvider?.Highlight1 ?? colours.Green;
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                this.ResizeTo(new Vector2(SCROLL_BAR_WIDTH)
                {
                    [(int)ScrollDirection] = val
                }, duration, easing);
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeColour(hoverColour, 100);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeColour(defaultColour, 100);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!base.OnMouseDown(e)) return false;

                // note that we are changing the colour of the box here as to not interfere with the hover effect.
                box.FadeColour(highlightColour, 100);
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button != MouseButton.Left) return;

                box.FadeColour(Color4.White, 100);

                base.OnMouseUp(e);
            }
        }
    }
}
