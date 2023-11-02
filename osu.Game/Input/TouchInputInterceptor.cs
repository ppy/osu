// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osuTK;
using osuTK.Input;

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
            bool touchInputWasActive = statics.Get<bool>(Static.TouchInputActive);

            switch (e)
            {
                case MouseEvent:
                    if (e.CurrentState.Mouse.LastSource is not ISourcedFromTouch)
                    {
                        if (touchInputWasActive)
                            Logger.Log($@"Touch input deactivated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                        statics.SetValue(Static.TouchInputActive, false);
                    }

                    break;

                case TouchEvent:
                    if (!touchInputWasActive)
                        Logger.Log($@"Touch input activated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                    statics.SetValue(Static.TouchInputActive, true);
                    break;

                case KeyDownEvent keyDown:
                    if (keyDown.Key == Key.T && keyDown.ControlPressed && keyDown.ShiftPressed)
                        debugToggleTouchInputActive();
                    break;
            }

            return false;
        }

        [Conditional("TOUCH_INPUT_DEBUG")]
        private void debugToggleTouchInputActive()
        {
            bool oldValue = statics.Get<bool>(Static.TouchInputActive);
            bool newValue = !oldValue;
            Logger.Log($@"Debug-toggling touch input to {(newValue ? @"active" : @"inactive")}", LoggingTarget.Input, LogLevel.Debug);
            statics.SetValue(Static.TouchInputActive, newValue);
        }
    }
}
