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
        /// The maximum amount of touches that should be allowed.
        /// </summary>
        private const int allowed_touches_limit = 3;

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
        public HashSet<TouchSource> ActiveTapTouches = new HashSet<TouchSource>();

        /// <summary>
        /// Tracks the amount of active touches.
        /// </summary>
        private int activeTouchesAmount => osuInputManager.CurrentState.Touch.ActiveSources.Count();

        /// <summary>
        /// Tracks whether we just entered a tap only mapping state.
        /// </summary>
        public bool EnteredTapOnlyMapping;

        /// <summary>
        /// Used to track whether the mapped inputs will only map for tap touches, this only happens if there are <see cref="allowed_touches_limit"/> touches being pressed or more.
        /// </summary>
        public bool TapOnlyMapping => activeTouchesAmount >= allowed_touches_limit;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Where(source => (source != TouchSource.Touch1 || !osuInputManager.AllowUserCursorMovement)))
                tapTouchActions.Add(source, (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        public bool IsTapTouch(TouchSource source) => tapTouchActions.ContainsKey(source);

        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        /// <summary>
        /// Whether we didn't reached the limit of allowed simultaneous touches.
        /// </summary>
        public bool AllowingOtherTouch => activeTouchesAmount <= allowed_touches_limit;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;

            if (!AllowingOtherTouch)
                return base.OnTouchDown(e);

            if (IsTapTouch(source))
            {
                if (TapOnlyMapping)
                {
                    if (!EnteredTapOnlyMapping)
                    {
                        EnteredTapOnlyMapping = true;
                        if (osuInputManager.CurrentState.Touch.IsActive(TouchSource.Touch1))
                            osuInputManager.HandleTouchTapOnlyMapping();
                    }
                }
                else
                {
                    EnteredTapOnlyMapping = false;
                }

                ActiveTapTouches.Add(source);
                keyBindingContainer.TriggerPressed(tapTouchActions[source]);
            }

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            if (ActiveTapTouches.Contains(source))
            {
                ActiveTapTouches.Remove(source);
                keyBindingContainer.TriggerReleased(tapTouchActions[source]);
            }

            base.OnTouchUp(e);
        }
    }
}
