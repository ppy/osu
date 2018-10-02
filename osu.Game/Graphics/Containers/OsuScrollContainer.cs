// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using OpenTK.Input;

namespace osu.Game.Graphics.Containers
{
    public class OsuScrollContainer : ScrollContainer
    {
        /// <summary>
        /// Allows controlling the scroll bar from any position in the container using the right mouse button.
        /// Uses the value of <see cref="DistanceDecayOnRightMouseScrollbar"/> to smoothly scroll to the dragged location.
        /// </summary>
        public bool RightMouseScrollbar = false;

        /// <summary>
        /// Controls the rate with which the target position is approached when performing a relative drag. Default is 0.02.
        /// </summary>
        public double DistanceDecayOnRightMouseScrollbar = 0.02;

        private bool shouldPerformRightMouseScroll(InputState state) => RightMouseScrollbar && state.Mouse.IsPressed(MouseButton.Right);

        private void scrollToRelative(float value) => ScrollTo(Clamp((value - Scrollbar.DrawSize[ScrollDim] / 2) / Scrollbar.Size[ScrollDim]), true, DistanceDecayOnRightMouseScrollbar);

        private bool mouseScrollBarDragging;

        protected override bool IsDragging => base.IsDragging || mouseScrollBarDragging;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (shouldPerformRightMouseScroll(state))
            {
                scrollToRelative(state.Mouse.Position[ScrollDim]);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override bool OnDrag(DragEvent e)
        {
            if (mouseScrollBarDragging)
            {
                scrollToRelative(e.Mouse.Position[ScrollDim]);
                return true;
            }

            return base.OnDrag(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (shouldPerformRightMouseScroll(e))
            {
                mouseScrollBarDragging = true;
                return true;
            }

            return base.OnDragStart(e);
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            if (mouseScrollBarDragging)
            {
                mouseScrollBarDragging = false;
                return true;
            }

            return base.OnDragEnd(e);
        }
    }
}
