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
        /// Checks whether given tap touches count is supposed to trigger tap only mapping. 
        /// </summary>
        /// <param name="count">The count to check agains the <see cref="allowed_tap_touches_limit"/></param>
        /// <returns></returns>
        private bool checkTapOnlyMapping(int count) => count >= allowed_tap_touches_limit;

        /// <summary>
        /// Tracks whether the mapped inputs will only map for tap touches, this only happens if there are <see cref="allowed_tap_touches_limit"/> touches being pressed or more.
        /// </summary>
        public bool TapOnlyMapping => checkTapOnlyMapping(ActiveTapTouchesCount);

        /// <summary>
        /// Tracks wheter the next touch will begin <see cref="TapOnlyMapping"/>.
        /// </summary>
        public bool NextTouchWillBeTapOnlyMapping => checkTapOnlyMapping(ActiveTapTouchesCount + 1);

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>())
                tapTouchActions.Add(source, (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        public bool IsTapTouch(TouchSource source) => source != TouchSource.Touch1 || !osuInputManager.AllowUserCursorMovement;

        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        /// <summary>
        /// Whether we didn't reached the limit of allowed simultaneous touches.
        /// </summary>
        public bool AllowingOtherTouch => ActiveTapTouchesCount <= allowed_tap_touches_limit;

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
