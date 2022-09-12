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
        /// <summary>
        /// The default cursor touch it may fail <see cref="IsCursorTouch(TouchSource)"/> on cases where there aren't any cursor touches, such as when autopilot is on.
        /// </summary>
        public const TouchSource DEFAULT_CURSOR_TOUCH = TouchSource.Touch1;

        /// <summary>
        /// The maximum amount of tap touches that should be allowed.
        /// </summary>
        private const int allowed_tap_touches_limit = 2;

        /// <summary>
        /// The maximum amount of tap touches that should be allowed decremented. this is useful for calculations that require to account for the next touch.
        /// </summary>
        private const int allowed_tap_touches_limit_decremented = allowed_tap_touches_limit - 1;

        /// <summary>
        /// Our parent <see cref="OsuInputManager"/>
        /// </summary>
        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// Our <see cref="KeyBindingContainer"/> used to trigger the touch actions from the <see cref="tapTouchActions"/>.
        /// </summary>
        private KeyBindingContainer<OsuAction> keyBindingContainer => osuInputManager.KeyBindingContainer;

        /// <summary>
        /// A dictionary that maps <see cref="TouchSource"/> into a respective <see cref="OsuAction"/>.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> tapTouchActions = new Dictionary<TouchSource, OsuAction>();

        /// <summary>
        /// Tracks all active tap <see cref="TouchSource"/>s that can be mapped into a given <see cref="OsuAction"/> through the <see cref="tapTouchActions"/>.
        /// </summary>
        private readonly HashSet<TouchSource> activeTapTouches = new HashSet<TouchSource>();

        public int ActiveTapTouchesCount => activeTapTouches.Count;

        /// <summary>
        /// Tracks whether we just entered a tap only mapping state.
        /// </summary>
        public bool EnteredTapOnlyMapping;

        /// <summary>
        /// Checks whether tap touch only mapping would be triggered by a given limit.
        /// </summary>
        /// <param name="limit">The limit that the active tap touches should be to enter tap only mapping.</param>
        /// <returns>Whether the active tap touches is greater or equal to the given limit</returns>
        private bool checkTapOnlyMapping(int limit) => ActiveTapTouchesCount >= limit;

        /// <summary>
        /// Tracks whether the mapped inputs will only map for tap touches, this only happens if there are <see cref="allowed_tap_touches_limit"/> touches being pressed or more.
        /// </summary>
        public bool TapOnlyMapping => checkTapOnlyMapping(allowed_tap_touches_limit);

        /// <summary>
        /// Tracks wheter the next touch will begin <see cref="TapOnlyMapping"/>.
        /// </summary>
        public bool NextTouchWillBeTapOnlyMapping => checkTapOnlyMapping(allowed_tap_touches_limit_decremented);

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>())
                tapTouchActions.Add(source, (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a tap touch and should be mapped into a <see cref="OsuAction"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Whether the given source is a tap.</returns>
        public bool IsTapTouch(TouchSource source) => source != TouchSource.Touch1 || !osuInputManager.AllowUserCursorMovement;

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a cursor touch and shouldn't be mapped into a <see cref="OsuAction"/> by us.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Whether the given source is the cursor.</returns>
        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        /// <summary>
        /// Whether we won't reach the limit of tap touches by adding another touch.
        /// </summary>
        public bool AllowingOtherTouch => ActiveTapTouchesCount <= allowed_tap_touches_limit_decremented;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;

            if (!AllowingOtherTouch)
                return base.OnTouchDown(e);

            if (IsTapTouch(source))
            {
                if (NextTouchWillBeTapOnlyMapping)
                {
                    if (!EnteredTapOnlyMapping && osuInputManager.CurrentState.Touch.IsActive(DEFAULT_CURSOR_TOUCH) && IsCursorTouch(DEFAULT_CURSOR_TOUCH))
                    {
                        EnteredTapOnlyMapping = true;
                        osuInputManager.HandleTouchTapOnlyMapping();
                    }
                }
                else
                {
                    EnteredTapOnlyMapping = false;
                }

                activeTapTouches.Add(source);
                keyBindingContainer.TriggerPressed(tapTouchActions[source]);
            }

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            if (activeTapTouches.Contains(source))
            {
                activeTapTouches.Remove(source);
                keyBindingContainer.TriggerReleased(tapTouchActions[source]);
            }

            base.OnTouchUp(e);
        }
    }
}
