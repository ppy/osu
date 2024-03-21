// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineDragBox : DragBox
    {
        public double MinTime { get; private set; }

        public double MaxTime { get; private set; }

        private double? startTime;

        [Resolved]
        private Timeline timeline { get; set; } = null!;

        protected override Drawable CreateBox() => new Box
        {
            RelativeSizeAxes = Axes.Y,
            Alpha = 0.3f
        };

        public override void HandleDrag(MouseButtonEvent e)
        {
            startTime ??= timeline.TimeAtPosition(e.MouseDownPosition.X);
            double endTime = timeline.TimeAtPosition(e.MousePosition.X);

            MinTime = Math.Min(startTime.Value, endTime);
            MaxTime = Math.Max(startTime.Value, endTime);

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
