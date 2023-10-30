// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Input
{
    /// <summary>
    /// Intercepts all positional input events and sets the appropriate <see cref="Static.TouchInputActive"/> value
    /// for consumption by particular game screens.
    /// </summary>
    public partial class TouchInputInterceptor : Component
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        [Resolved]
        private SessionStatics statics { get; set; } = null!;

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case MouseEvent:
                    if (e.CurrentState.Mouse.LastSource is not ISourcedFromTouch)
                        statics.SetValue(Static.TouchInputActive, false);
                    break;

                case TouchEvent:
                    statics.SetValue(Static.TouchInputActive, true);
                    break;
            }

            return false;
        }
    }
}
