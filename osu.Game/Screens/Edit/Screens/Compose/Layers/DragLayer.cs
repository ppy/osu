// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
    /// A layer that handles and displays drag selection for a collection of <see cref="HitObjectMask"/>s.
    /// </summary>
    public class DragLayer : CompositeDrawable
    {
        private readonly Action<RectangleF> performSelection;

        /// <summary>
        /// Invoked when the drag selection has finished.
        /// </summary>
        public event Action DragEnd;

        private Drawable box;

        /// <summary>
        /// Creates a new <see cref="DragLayer"/>.
        /// </summary>
        /// <param name="maskContainer">The selectable <see cref="HitObjectMask"/>s.</param>
        public DragLayer(Action<RectangleF> performSelection)
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
                BorderThickness = MaskSelection.BORDER_RADIUS,
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

            performSelection?.Invoke(dragRectangle);
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
