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
    public partial class OsuTouchInputMapper : Drawable
    {
        public const TouchSource DEFAULT_CURSOR_TOUCH = TouchSource.Touch1;

        /// <summary>
        /// The max amount of tap touches that should be allowed.
        /// </summary>
        private const int allowed_tap_touches_limit = 2;

        /// <summary>
        /// The max amount of tap touches that should be allowed decremented. can be used on cases like <see cref="BlockCursorActionOnNextTap"/>.
        /// </summary>
        private const int allowed_tap_touches_limit_decremented = allowed_tap_touches_limit - 1;

        private readonly OsuInputManager osuInputManager;

        private KeyBindingContainer<OsuAction> keyBindingContainer => osuInputManager.KeyBindingContainer;

        /// <summary>
        /// Maps all <see cref="TouchSource"/>s into a respective <see cref="OsuAction"/>.
        /// </summary>
        private readonly Dictionary<TouchSource, OsuAction> tapActionsMap = Enum.GetValues(typeof(TouchSource))
                                                                                .Cast<TouchSource>()
                                                                                .ToDictionary(source => source, source => (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);

        /// <summary>
        /// Tracks all currently active tap <see cref="TouchSource"/>s.
        /// </summary>
        private readonly HashSet<TouchSource> activeTapTouches = new HashSet<TouchSource>();

        /// <summary>
        /// Tracks how many tap touches are currently active.
        /// </summary>
        public int ActiveTapTouchesCount => activeTapTouches.Count;

        /// <summary>
        /// Checks whether we should block the <see cref="OsuAction.LeftButton"/> triggered by the <see cref="DEFAULT_CURSOR_TOUCH"/> from being propagated.
        /// </summary>
        /// <param name="limit">How many tap touches for the cursor actions to be blocked</param>
        /// <returns>Whether the <see cref="OsuAction.LeftButton"/> should be blocked from being propagated.</returns>
        private bool checkBlockCursorAction(int limit) => ActiveTapTouchesCount >= limit;

        public bool BlockCursorAction => checkBlockCursorAction(allowed_tap_touches_limit);

        public bool BlockCursorActionOnNextTap => checkBlockCursorAction(allowed_tap_touches_limit_decremented);

        /// <summary>
        /// Whether we just blocked the cursor actions from being propagated.
        /// </summary>
        public bool JustBlockedCursorActions;

        public OsuTouchInputMapper(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
        }

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a tap touch.
        /// <remarks>
        /// A tap touch is any touch that is done whilst a cursor touch is active or when the cursor movement is blocked.
        /// </remarks>
        /// </summary>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>Whether the given source is a tap touch.</returns>
        public bool IsTapTouch(TouchSource source) => source != DEFAULT_CURSOR_TOUCH || !osuInputManager.AllowUserCursorMovement;

        /// <summary>
        /// Checks whether a given <see cref="TouchSource"/> is a cursor touch.
        /// </summary>
        /// <remarks>
        /// The <see cref="DEFAULT_CURSOR_TOUCH"/> will always be the cursor touch.
        /// Unless the cursor movement is blocked, in this case there aren't cursor touches.
        /// </remarks>
        /// <param name="source">The <see cref="TouchSource"/> to check.</param>
        /// <returns>Whether the given source is the cursor touch.</returns>
        public bool IsCursorTouch(TouchSource source) => !IsTapTouch(source);

        /// <summary>
        /// Whether we can still handle another touch input.
        /// </summary>
        public bool AcceptingTouchInputs => ActiveTapTouchesCount <= allowed_tap_touches_limit_decremented;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;

            if (!AcceptingTouchInputs || IsCursorTouch(source)) return base.OnTouchDown(e);

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
            keyBindingContainer.TriggerPressed(tapActionsMap[source]);

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;

            // We must only disable active tap touches. this guarantees that their assigned actions were properly triggered and can be disabled.
            if (activeTapTouches.Contains(source))
            {
                activeTapTouches.Remove(source);
                keyBindingContainer.TriggerReleased(tapActionsMap[source]);
            }

            base.OnTouchUp(e);
        }
    }
}
