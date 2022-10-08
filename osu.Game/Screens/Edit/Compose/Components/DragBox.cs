// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A box that displays the drag selection and provides selection events for users to handle.
    /// </summary>
    public class DragBox : CompositeDrawable, IStateful<Visibility>
    {
        protected readonly Action<RectangleF> PerformSelection;

        protected Drawable Box;

        /// <summary>
        /// Creates a new <see cref="DragBox"/>.
        /// </summary>
        /// <param name="performSelection">A delegate that performs drag selection.</param>
        public DragBox(Action<RectangleF> performSelection)
        {
            PerformSelection = performSelection;

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = Box = CreateBox();
        }

        protected virtual Drawable CreateBox() => new BoxWithBorders();

        private RectangleF? dragRectangle;

        /// <summary>
        /// Handle a forwarded mouse event.
        /// </summary>
        /// <param name="e">The mouse event.</param>
        /// <returns>Whether the event should be handled and blocking.</returns>
        public virtual bool HandleDrag(MouseButtonEvent e)
        {
            var dragPosition = e.ScreenSpaceMousePosition;
            var dragStartPosition = e.ScreenSpaceMouseDownPosition;

            var dragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            // We use AABBFloat instead of RectangleF since it handles negative sizes for us
            var rec = dragQuad.AABBFloat;
            dragRectangle = rec;

            var topLeft = ToLocalSpace(rec.TopLeft);
            var bottomRight = ToLocalSpace(rec.BottomRight);

            Box.Position = topLeft;
            Box.Size = bottomRight - topLeft;
            return true;
        }

        private Visibility state;

        public Visibility State
        {
            get => state;
            set
            {
                if (value == state) return;

                state = value;
                this.FadeTo(state == Visibility.Hidden ? 0 : 1, 250, Easing.OutQuint);
                StateChanged?.Invoke(state);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (dragRectangle != null)
                PerformSelection?.Invoke(dragRectangle.Value);
        }

        public override void Hide()
        {
            State = Visibility.Hidden;
            dragRectangle = null;
        }

        public override void Show() => State = Visibility.Visible;

        public event Action<Visibility> StateChanged;

        public class BoxWithBorders : CompositeDrawable
        {
            private readonly LayoutValue cache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

            public BoxWithBorders()
            {
                AddLayout(cache);
            }

            protected override void Update()
            {
                base.Update();

                if (!cache.IsValid)
                {
                    createContent();
                    cache.Validate();
                }
            }

            private void createContent()
            {
                if (DrawSize == Vector2.Zero)
                {
                    ClearInternal();
                    return;
                }

                // Make lines the same width independent of display resolution.
                float lineThickness = DrawWidth > 0
                    ? DrawWidth / ScreenSpaceDrawQuad.Width * 2
                    : DrawHeight / ScreenSpaceDrawQuad.Height * 2;

                Padding = new MarginPadding(-lineThickness / 2);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = lineThickness,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = lineThickness,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = lineThickness,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = lineThickness,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f
                    }
                };
            }
        }
    }
}
