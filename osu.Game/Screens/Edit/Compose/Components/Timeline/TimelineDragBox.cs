// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineDragBox : DragBox
    {
        public double MinTime => Math.Min(startTime.Value, endTime);

        public double MaxTime => Math.Max(startTime.Value, endTime);

        private double? startTime;

        private double endTime;

        [Resolved]
        private Timeline timeline { get; set; }

        protected override Drawable CreateBox() => new Box
        {
            RelativeSizeAxes = Axes.Y,
            Alpha = 0.3f
        };

        public override void HandleDrag(MouseButtonEvent e)
        {
            startTime ??= timeline.TimeAtPosition(e.MouseDownPosition.X);
            endTime = timeline.TimeAtPosition(e.MousePosition.X);

            Box.X = timeline.PositionAtTime(MinTime);
            Box.Width = timeline.PositionAtTime(MaxTime) - Box.X;
        }

        public override void Hide()
        {
            base.Hide();
            startTime = null;
        }
    }
}
