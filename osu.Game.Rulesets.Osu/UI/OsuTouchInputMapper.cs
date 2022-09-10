// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuTouchInputMapper : Drawable
    {
        public const TouchSource CURSOR_TOUCH = TouchSource.Touch1;

        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// The maximum amount of touches that should be allowed.
        /// </summary>
        private const int allowed_touches_limit = 3;

        /// <summary>
        /// The last touch index that is allowed to map to a given <see cref="OsuAction"/>.
        /// </summary>
        private const int last_allowed_touch_index = allowed_touches_limit - 1;

        /// <summary>
        /// Our <see cref="KeyBindingContainer"/> used to trigger the touch actions from the <see cref="touchActions"/>.
        /// </summary>
        private KeyBindingContainer<OsuAction> keyBindingContainer => osuInputManager.KeyBindingContainer;

        /// <summary>
        /// A dictionary that maps <see cref="TouchSource"/> into a respective <see cref="OsuAction"/> for an emulated keyboard input.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> touchActions = new Dictionary<TouchSource, OsuAction>();

        /// <summary>
        /// Tracks the amount of active touches.
        /// </summary>
        /// <remarks>
        /// Since <see cref="OsuInputManager"/>'s active sources uses an internal hash set, the time complexity for this should be o(n)
        /// </remarks>
        private int activeTouchesAmount => osuInputManager.CurrentState.Touch.ActiveSources.Count();

        /// <summary>
        /// When we enter drag cursor mode, the cursor will stop being mapped to a <see cref="OsuAction"/> and the other valid touches can be used as streaming keys.
        /// </summary>
        public bool DraggingCursorMode => activeTouchesAmount >= allowed_touches_limit;

        private int getTouchIndex(TouchSource source) => source - TouchSource.Touch1;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Take(allowed_touches_limit))
                touchActions.Add(source, getTouchIndex(source) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        public bool IsTapTouch(TouchSource source) => source != CURSOR_TOUCH || !osuInputManager.AllowUserCursorMovement;

        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        public bool IsTouchAllowed(TouchSource source) => getTouchIndex(source) <= last_allowed_touch_index;

        public bool IsAllowedTapTouch(TouchSource source) => IsTouchAllowed(source) && IsTapTouch(source);

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;

            if (IsAllowedTapTouch(source))
                keyBindingContainer.TriggerPressed(touchActions[source]);

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            if (IsAllowedTapTouch(source))
                keyBindingContainer.TriggerReleased(touchActions[source]);

            base.OnTouchUp(e);
        }
    }
}
