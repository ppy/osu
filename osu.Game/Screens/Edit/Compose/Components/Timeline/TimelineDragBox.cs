// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineDragBox : DragBox
    {
        // the following values hold the start and end X positions of the drag box in the timeline's local space,
        // but with zoom unapplied in order to be able to compensate for positional changes
        // while the timeline is being zoomed in/out.
        private float? selectionStart;
        private float selectionEnd;

        [Resolved]
        private Timeline timeline { get; set; }

        public TimelineDragBox(Action<RectangleF> performSelect)
            : base(performSelect)
        {
        }

        protected override Drawable CreateBox() => new Box
        {
            RelativeSizeAxes = Axes.Y,
            Alpha = 0.3f
        };

        public override bool HandleDrag(MouseButtonEvent e)
        {
            // The dragbox should only be active if the mouseDownPosition.Y is within this drawable's bounds.
            float localY = ToLocalSpace(e.ScreenSpaceMouseDownPosition).Y;
            if (DrawRectangle.Top > localY || DrawRectangle.Bottom < localY)
                return false;

            selectionStart ??= e.MouseDownPosition.X / timeline.CurrentZoom;

            // only calculate end when a transition is not in progress to avoid bouncing.
            if (Precision.AlmostEquals(timeline.CurrentZoom, timeline.Zoom))
                selectionEnd = e.MousePosition.X / timeline.CurrentZoom;

            updateDragBoxPosition();
            return true;
        }

        private void updateDragBoxPosition()
        {
            if (selectionStart == null)
                return;

            float rescaledStart = selectionStart.Value * timeline.CurrentZoom;
            float rescaledEnd = selectionEnd * timeline.CurrentZoom;

            Box.X = Math.Min(rescaledStart, rescaledEnd);
            Box.Width = Math.Abs(rescaledStart - rescaledEnd);

            var boxScreenRect = Box.ScreenSpaceDrawQuad.AABBFloat;

            // we don't care about where the hitobjects are vertically. in cases like stacking display, they may be outside the box without this adjustment.
            boxScreenRect.Y -= boxScreenRect.Height;
            boxScreenRect.Height *= 2;

            PerformSelection?.Invoke(boxScreenRect);
        }

        public override void Hide()
        {
            base.Hide();
            selectionStart = null;
        }
    }
}
