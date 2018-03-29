using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Rulesets.Edit;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    /// <summary>
    /// A box that handles and displays drag selection for a collection of <see cref="HitObjectMask"/>s.
    /// </summary>
    public class DragBox : CompositeDrawable
    {
        /// <summary>
        /// Invoked when the drag selection has finished.
        /// </summary>
        public event Action DragEnd;

        private readonly IEnumerable<HitObjectMask> hitObjectMasks;

        private Drawable box;

        /// <summary>
        /// Creates a new <see cref="DragBox"/>.
        /// </summary>
        /// <param name="hitObjectMasks">The selectable <see cref="HitObjectMask"/>s.</param>
        public DragBox(IEnumerable<HitObjectMask> hitObjectMasks)
        {
            this.hitObjectMasks = hitObjectMasks;

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
                BorderThickness = SelectionBox.BORDER_RADIUS,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f
                }
            };
        }

        protected override bool OnDragStart(InputState state)
        {
            this.FadeIn(250, Easing.OutQuint);
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var dragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            // We use AABBFloat instead of RectangleF since it handles negative sizes for us
            var dragRectangle = dragQuad.AABBFloat;

            var topLeft = ToLocalSpace(dragRectangle.TopLeft);
            var bottomRight = ToLocalSpace(dragRectangle.BottomRight);

            box.Position = topLeft;
            box.Size = bottomRight - topLeft;

            foreach (var mask in hitObjectMasks)
            {
                if (mask.IsAlive && mask.IsPresent && dragRectangle.Contains(mask.SelectionPoint))
                    mask.Select();
                else
                    mask.Deselect();
            }

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            this.FadeOut(250, Easing.OutQuint);
            DragEnd?.Invoke();
            return true;
        }
    }
}
