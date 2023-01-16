// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuTouchInputMapper : Drawable
    {
        /// <summary>
        /// This is our parent <see cref="osuInputManager"/>.
        /// </summary>
        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// All the active <see cref="TouchSource"/>s and the <see cref="OsuAction"/> that it triggered (if any).
        /// Ordered from oldest to newest touch chronologically.
        /// </summary>
        private readonly List<TrackedTouch> trackedTouches = new List<TrackedTouch>();

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
        }

        private OsuAction? lastAction;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            OsuAction action = lastAction == OsuAction.LeftButton && trackedTouches.Count > 0 ? OsuAction.RightButton : OsuAction.LeftButton;

            if (trackedTouches.All(t => t.Action != action))
            {
                trackedTouches.Add(new TrackedTouch(e.Touch, action));
                osuInputManager.KeyBindingContainer.TriggerPressed(action);
                lastAction = action;
            }
            else
            {
                // Ignore any taps which trigger an action which is already handled. But track them for potential positional input in the future.
                trackedTouches.Add(new TrackedTouch(e.Touch, null));
            }

            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var tracked = trackedTouches.First(t => t.Touch.Source == e.Touch.Source);

            if (tracked.Action is OsuAction action)
                osuInputManager.KeyBindingContainer.TriggerReleased(action);

            trackedTouches.Remove(tracked);

            base.OnTouchUp(e);
        }

        private class TrackedTouch
        {
            public readonly Touch Touch;

            public readonly OsuAction? Action;

            public TrackedTouch(Touch touch, OsuAction? action)
            {
                Touch = touch;
                Action = action;
            }
        }
    }
}
