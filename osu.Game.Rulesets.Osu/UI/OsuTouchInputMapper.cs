// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuTouchInputMapper : Drawable
    {
        public const TouchSource CURSOR_TOUCH = TouchSource.Touch1;

        /// <summary>
        /// How many streaming touches are allowed to be registered.
        /// </summary>
        private const int tap_touches_limit = 2;

        /// <summary>
        /// How many touches are allowed to be registered.
        /// </summary>
        private const int allowed_touches_limit = tap_touches_limit + 1;

        /// <summary>
        /// The index for the last allowed touch.
        /// </summary>
        private const int last_allowed_touch_index = allowed_touches_limit - 1;

        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// A hash set that contains all the <see cref="TouchSource"/>'s that can be mapped to a given <see cref="OsuAction"/>.
        /// </summary>
        private readonly HashSet<TouchSource> allowedTouches = Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Take(allowed_touches_limit).ToHashSet();

        /// <summary>
        /// A hash set that contains all the <see cref="allowedTouches"/> that are currently active.
        /// </summary>
        /// <remarks>
        /// Although inputs that aren't valid are being blocked from being passed to us by our <see cref="osuInputManager"/>
        /// this does not mean that the <see cref="osuInputManager"/>'s active touches only contains valid touches, that's the reason for this to exist.
        /// </remarks>
        private readonly HashSet<TouchSource> activeAllowedTouches = new HashSet<TouchSource>();

        /// <summary>
        /// A dictionary that maps <see cref="TouchSource"/> into a respective <see cref="OsuAction"/> for an emulated keyboard input.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> touchActions = new Dictionary<TouchSource, OsuAction>();

        /// <summary>
        /// When we enter drag cursor mode, the cursor will stop being mapped to a <see cref="OsuAction"/> and the other valid touches can be used as streaming keys.
        /// </summary>
        public bool DraggingCursorMode;

        private int getTouchIndex(TouchSource source) => source - TouchSource.Touch1;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in allowedTouches)
                touchActions.Add(source, getTouchIndex(source) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        public bool IsTapTouch(TouchSource source) => source != CURSOR_TOUCH || !osuInputManager.AllowUserCursorMovement;

        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        public bool IsTouchBlocked(TouchSource source) => getTouchIndex(source) > last_allowed_touch_index;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;

            if (IsTouchBlocked(source))
                return true;

            activeAllowedTouches.Add(source);
            DraggingCursorMode = activeAllowedTouches.Count == allowedTouches.Count;

            if (IsCursorTouch(source))
                return base.OnTouchDown(e);

            osuInputManager.KeyBindingContainer.TriggerPressed(touchActions[source]);

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            if (IsTouchBlocked(source))
                return;

            activeAllowedTouches.Remove(source);

            if (IsTapTouch(source))
                osuInputManager.KeyBindingContainer.TriggerReleased(touchActions[source]);

            base.OnTouchUp(e);
        }
    }
}
