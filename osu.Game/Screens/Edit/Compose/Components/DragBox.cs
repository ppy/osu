// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A box that displays the drag selection and provides selection events for users to handle.
    /// </summary>
    public class DragBox : CompositeDrawable
    {
        private readonly Action<RectangleF> performSelection;

        private Drawable box;

        /// <summary>
        /// Creates a new <see cref="DragBox"/>.
        /// </summary>
        /// <param name="performSelection">A delegate that performs drag selection.</param>
        public DragBox(Action<RectangleF> performSelection)
        {
            this.performSelection = performSelection;

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = box = new Container
            {
                Masking = true,
                BorderColour = Color4.White,
                BorderThickness = SelectionHandler.BORDER_RADIUS,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f
                }
            };
        }

        public void UpdateDrag(MouseButtonEvent e)
        {
            var dragPosition = e.ScreenSpaceMousePosition;
            var dragStartPosition = e.ScreenSpaceMouseDownPosition;

            var dragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            // We use AABBFloat instead of RectangleF since it handles negative sizes for us
            var dragRectangle = dragQuad.AABBFloat;

            var topLeft = ToLocalSpace(dragRectangle.TopLeft);
            var bottomRight = ToLocalSpace(dragRectangle.BottomRight);

            box.Position = topLeft;
            box.Size = bottomRight - topLeft;

            performSelection?.Invoke(dragRectangle);
        }
    }
}
