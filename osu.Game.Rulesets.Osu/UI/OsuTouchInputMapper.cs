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
        /// The cursor touch represents the <see cref="TouchSource"/> that controls whether your cursor is at the screen, this being the first touch.
        /// on mods where you can't control your cursor with touch input (e.g autopilot) it isn't considered being a "cursor touch" therefore it won't
        /// return true when calling <see cref="IsCursorTouch(TouchSource)"/> and will be considered a tap touch instead.
        /// </summary>
        public const TouchSource DEFAULT_CURSOR_TOUCH = TouchSource.Touch1;

        /// <summary>
        /// The maximum amount of tap touches that should be allowed.
        /// we should ignore any taps that are added beyond this limit.
        /// </summary>
        private const int allowed_tap_touches_limit = 2;

        /// <summary>
        /// The maximum amount of tap touches that should be allowed decremented. this is useful for calculations that require to account for the next touch.
        /// </summary>
        private const int allowed_tap_touches_limit_decremented = allowed_tap_touches_limit - 1;

        /// <summary>
        /// Our parent <see cref="OsuInputManager"/>.
        /// </summary>
        private readonly OsuInputManager osuInputManager;

        /// <summary>
        /// Our <see cref="KeyBindingContainer"/> used to trigger the touch actions from the <see cref="tapTouchActions"/>.
        /// </summary>
        private KeyBindingContainer<OsuAction> keyBindingContainer => osuInputManager.KeyBindingContainer;

        /// <summary>
        /// A dictionary that maps <see cref="TouchSource"/> into a respective <see cref="OsuAction"/>.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> tapTouchActions = Enum.GetValues(typeof(TouchSource))
                                                                                  .Cast<TouchSource>()
                                                                                  .ToDictionary(source => source, source => (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);

        /// <summary>
        /// Tracks all currently active tap <see cref="TouchSource"/>s that can be mapped into a given <see cref="OsuAction"/> through the <see cref="tapTouchActions"/>.
        /// </summary>
        private readonly HashSet<TouchSource> activeTapTouches = new HashSet<TouchSource>();

        /// <summary>
        /// Tracks how many active tap touches are active currently.
        /// </summary>
        public int ActiveTapTouchesCount => activeTapTouches.Count;

        /// <summary>
        /// Tracks whether we just blocked the cursor actions. so we don't try to block the cursor actions when they are already blocked.
        /// </summary>
        public bool JustBlockedCursorActions;

        /// <summary>
        /// Checks whether the <see cref="OsuAction.LeftButton"/> propagated by the cursor touch should be blocked by a given limit of active tap touches.
        /// </summary>
        /// <param name="limit">How many tap touches are necessary for the cursor actions to be blocked</param>
        /// <returns>Whether the <see cref="OsuAction.LeftButton"/> should be blocked from being propagated by the cursor touch.</returns>
        private bool checkBlockCursorAction(int limit) => ActiveTapTouchesCount >= limit;

        /// <summary>
        /// Tracks whether the <see cref="OsuAction.LeftButton"/> that is propagated by the cursor touch should be blocked.
        /// this allows for all the tapping work to be handled by the tap <see cref="TouchSource"/>s
        /// </summary>
        public bool BlockCursorAction => checkBlockCursorAction(allowed_tap_touches_limit);

        /// <summary>
        /// Tracks wheter the next tap <see cref="TouchSource"/> will trigger <see cref="BlockCursorAction"/>.
        /// </summary>
        public bool BlockCursorActionOnNextTap => checkBlockCursorAction(allowed_tap_touches_limit_decremented);

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
        }

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a tap touch and should be mapped into a <see cref="OsuAction"/>.
        /// </summary>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>Whether the given source is a tap.</returns>
        public bool IsTapTouch(TouchSource source) => source != TouchSource.Touch1 || !osuInputManager.AllowUserCursorMovement;

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a cursor touch and shouldn't be mapped into a <see cref="OsuAction"/> by us.
        /// </summary>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>
        /// Whether the given source is the cursor touch, usually it must be <see cref="DEFAULT_CURSOR_TOUCH"/>
        /// it may not be the case with mods that block the cursor movement (e.g autopilot).
        /// </returns>
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
                if (BlockCursorActionOnNextTap)
                {
                    if (!JustBlockedCursorActions)
                        JustBlockedCursorActions = osuInputManager.BlockTouchCursorAction();
                }
                else
                {
                    JustBlockedCursorActions = false;
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
