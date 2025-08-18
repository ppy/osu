// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Graphics.Carousel
{
    /// <summary>
    /// A highly efficient vertical list display that is used primarily for the song select screen,
    /// but flexible enough to be used for other use cases.
    /// </summary>
    public abstract partial class Carousel<T> where T : notnull
    {
        /// <summary>
        /// Implementation of scroll container which handles very large vertical lists by internally using <c>double</c> precision
        /// for pre-display Y values.
        /// </summary>
        protected partial class ScrollContainer : UserTrackingScrollContainer, IKeyBindingHandler<GlobalAction>
        {
            public readonly Container Panels;

            public void SetLayoutHeight(float height) => Panels.Height = height;

            protected override ScrollbarContainer CreateScrollbar(Direction direction) => new ScrollBar();

            /// <summary>
            /// Allow handling right click scroll outside of the carousel's display area.
            /// </summary>
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public ScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with public ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(Panels = new Container
                {
                    Name = "Layout content",
                    RelativeSizeAxes = Axes.X,
                });
            }

            public override void OffsetScrollPosition(double offset)
            {
                base.OffsetScrollPosition(offset);

                foreach (var panel in Panels)
                    ((ICarouselPanel)panel).DrawYPosition += offset;
            }

            public override void Clear(bool disposeChildren)
            {
                Panels.Height = 0;
                Panels.Clear(disposeChildren);
            }

            public override void Add(Drawable drawable)
            {
                if (drawable is not ICarouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                Panels.Add(drawable);
            }

            public override double GetChildPosInContent(Drawable d, Vector2 offset)
            {
                if (d is not ICarouselPanel panel)
                    return base.GetChildPosInContent(d, offset);

                return panel.DrawYPosition + offset.X;
            }

            protected override void ApplyCurrentToContent()
            {
                Debug.Assert(ScrollDirection == Direction.Vertical);

                double scrollableExtent = -Current + ScrollableExtent * ScrollContent.RelativeAnchorPosition.Y;

                foreach (var d in Panels)
                    d.Y = (float)(((ICarouselPanel)d).DrawYPosition + scrollableExtent);
            }

            #region Scrollbar padding

            public float ScrollbarPaddingTop { get; set; } = 5;
            public float ScrollbarPaddingBottom { get; set; } = 5;

            protected override float ToScrollbarPosition(double scrollPosition)
            {
                if (Precision.AlmostEquals(0, ScrollableExtent))
                    return 0;

                return (float)(ScrollbarPaddingTop + (ScrollbarMovementExtent - (ScrollbarPaddingTop + ScrollbarPaddingBottom)) * (scrollPosition / ScrollableExtent));
            }

            protected override float FromScrollbarPosition(float scrollbarPosition)
            {
                if (Precision.AlmostEquals(0, ScrollbarMovementExtent))
                    return 0;

                return (float)(ScrollableExtent * ((scrollbarPosition - ScrollbarPaddingTop) / (ScrollbarMovementExtent - (ScrollbarPaddingTop + ScrollbarPaddingBottom))));
            }

            #endregion

            #region Absolute scrolling

            /// <summary>
            /// Whether absolute scrolling is currently triggered.
            /// </summary>
            public bool AbsoluteScrolling { get; private set; }

            protected override bool IsDragging => base.IsDragging || AbsoluteScrolling;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        beginAbsoluteScrolling(e);
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        endAbsoluteScrolling();
                        break;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Right)
                {
                    // To avoid conflicts with context menus, disallow absolute scroll if it looks like things will fall over.
                    if (GetContainingInputManager()!.HoveredDrawables.OfType<IHasContextMenu>().Any())
                        return false;

                    beginAbsoluteScrolling(e);
                }

                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Right)
                    endAbsoluteScrolling();
                base.OnMouseUp(e);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                if (AbsoluteScrolling)
                {
                    ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                    return true;
                }

                return base.OnMouseMove(e);
            }

            private void beginAbsoluteScrolling(UIEvent e)
            {
                ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                AbsoluteScrolling = true;
            }

            private void endAbsoluteScrolling() => AbsoluteScrolling = false;

            #endregion

            #region Scrollbar

            private partial class ScrollBar : ScrollbarContainer
            {
                private Color4 hoverColour;
                private Color4 defaultColour;
                private Color4 highlightColour;

                private readonly Drawable box;

                protected override float MinimumDimSize => SCROLL_BAR_WIDTH * 3;

                private const float expanded_size_ratio = 2;

                public ScrollBar()
                    : base(Direction.Vertical)
                {
                    Blending = BlendingParameters.Additive;

                    // needs to be set initially for the ResizeTo to respect minimum size
                    Size = new Vector2(SCROLL_BAR_WIDTH * expanded_size_ratio, SCROLL_BAR_WIDTH);

                    const float margin = 3;

                    Margin = new MarginPadding
                    {
                        Left = margin,
                        Right = margin,
                    };

                    Child = box = new Circle
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Both,
                        Width = 1 / expanded_size_ratio,
                    };
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
                    this.ResizeTo(new Vector2(SCROLL_BAR_WIDTH * expanded_size_ratio)
                    {
                        [(int)ScrollDirection] = val
                    }, duration, easing);
                }

                protected override bool OnHover(HoverEvent e)
                {
                    updateVisuals(e);
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    updateVisuals(e);
                }

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    if (!base.OnMouseDown(e)) return false;

                    updateVisuals(e);
                    return true;
                }

                protected override void OnDragEnd(DragEndEvent e)
                {
                    updateVisuals(e);
                    base.OnDragEnd(e);
                }

                protected override void OnMouseUp(MouseUpEvent e)
                {
                    if (e.Button != MouseButton.Left) return;

                    updateVisuals(e);
                    base.OnMouseUp(e);
                }

                private void updateVisuals(MouseEvent e)
                {
                    if (IsDragged || e.PressedButtons.Contains(MouseButton.Left))
                        box.FadeColour(highlightColour, 100);
                    else if (IsHovered)
                        box.FadeColour(hoverColour, 100);
                    else
                        box.FadeColour(defaultColour, 100);

                    if (IsHovered || IsDragged)
                        box.ResizeWidthTo(1, 300, Easing.OutElasticHalf);
                    else
                        box.ResizeWidthTo(1 / expanded_size_ratio, 200, Easing.OutQuint);
                }
            }

            #endregion
        }
    }
}
