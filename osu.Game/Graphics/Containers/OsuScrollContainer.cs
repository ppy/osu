using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Graphics.Containers
{
    class OsuScrollContainer : ScrollContainer
    {
        /// <summary>
        /// Add the ability to seek to an absolute scroll position when the right mouse button is pressed or dragged.
        /// Uses the value of <see cref="DistanceDecayOnRightMouseScrollbar"/> to smoothly scroll to the dragged location.
        /// </summary>
        public bool RightMouseScrollbar = false;

        /// <summary>
        /// Controls the rate with which the target position is approached when performing a relative drag. Default is 0.02.
        /// </summary>
        public double DistanceDecayOnRightMouseScrollbar = 0.02;

        private bool shouldPerformRelativeDrag(InputState state) => RightMouseScrollbar && state.Mouse.IsPressed(MouseButton.Right);

        private void scrollToRelative(float value) => ScrollTo(Clamp((value - Scrollbar.DrawSize[ScrollDim] / 2) / Scrollbar.Size[ScrollDim]), true, DistanceDecayOnRightMouseScrollbar);

        private bool mouseScrollBarDragging;

        protected override bool IsDragging => base.IsDragging || mouseScrollBarDragging;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (shouldPerformRelativeDrag(state))
            {
                scrollToRelative(state.Mouse.Position[ScrollDim]);
                return true;
            }

            return base.OnMouseDown(state, args);
        }

        protected override bool OnDrag(InputState state)
        {
            if (mouseScrollBarDragging)
            {
                scrollToRelative(state.Mouse.Position[ScrollDim]);
                return true;
            }

            return base.OnDrag(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            if (shouldPerformRelativeDrag(state))
            {
                mouseScrollBarDragging = true;
                return true;
            }

            return base.OnDragStart(state);
        }

        protected override bool OnDragEnd(InputState state)
        {
            if (mouseScrollBarDragging)
            {
                mouseScrollBarDragging = false;
                return true;
            }

            return base.OnDragEnd(state);
        }
    }
}
