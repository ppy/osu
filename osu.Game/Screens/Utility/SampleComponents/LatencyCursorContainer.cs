// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility.SampleComponents
{
    public partial class LatencyCursorContainer : CursorContainer
    {
        protected override Drawable CreateCursor() => new LatencyCursor();

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        public LatencyCursorContainer()
        {
            State.Value = Visibility.Hidden;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // Scheduling is required to ensure updating of cursor position happens in limited rate.
            // We can alternatively solve this by a PassThroughInputManager layer inside LatencyArea,
            // but that would mean including input lag to this test, which may not be desired.
            Schedule(() => base.OnMouseMove(e));
            return false;
        }

        private partial class LatencyCursor : LatencySampleComponent
        {
            public LatencyCursor()
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;

                InternalChild = new Circle { Size = new Vector2(40) };
            }

            protected override void UpdateAtLimitedRate(InputState inputState)
            {
                Colour = inputState.Mouse.IsPressed(MouseButton.Left) ? OverlayColourProvider.Content1 : OverlayColourProvider.Colour2;
            }
        }
    }
}
