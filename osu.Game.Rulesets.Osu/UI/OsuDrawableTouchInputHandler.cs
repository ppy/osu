// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;

#nullable disable

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuDrawableTouchInputHandler : Drawable
    {
        public const TouchSource CURSOR_TOUCH = TouchSource.Touch1;
        public const OsuAction CURSOR_TOUCH_ACTION = OsuAction.LeftButton;

        /// <summary>
        /// How many taps (taps referring as streaming touch input) can be registered.
        /// </summary>
        public const int TAP_TOUCHES_LIMIT = 2;

        /// <summary>
        /// The index for the last concurrent able touch.
        /// </summary>
        public const int LAST_CONCURRENT_TOUCH_INDEX = TAP_TOUCHES_LIMIT;

        /// <summary>
        /// How many concurrent touches can be registered.
        /// </summary>
        public const int CONCURRENT_TOUCHES_LIMIT = LAST_CONCURRENT_TOUCH_INDEX + 1;

        public readonly HashSet<TouchSource> PossibleTapTouchSources = Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Take(CONCURRENT_TOUCHES_LIMIT).ToHashSet();

        private readonly Dictionary<TouchSource, OsuAction> touchTapActionsDictionary = new Dictionary<TouchSource, OsuAction>();

        private readonly OsuInputManager osuInputManager;

        private int getTouchIndex(TouchSource source)
        {
            return source - TouchSource.Touch1;
        }

        public OsuDrawableTouchInputHandler(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in PossibleTapTouchSources)
                touchTapActionsDictionary.Add(source, getTouchIndex(source) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);
        }

        private bool isTapTouch(TouchSource source)
        {
            return CURSOR_TOUCH != source || !osuInputManager.AllowUserCursorMovement;
        }

        private bool isCursorTouch(TouchSource source)
        {
            return !isTapTouch(source);
        }

        private bool isValidTouchInput(TouchSource source, int index)
        {
            return index <= LAST_CONCURRENT_TOUCH_INDEX;
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;
            int sourceIndex = getTouchIndex(source);

            osuInputManager.DragMode = sourceIndex >= LAST_CONCURRENT_TOUCH_INDEX;

            if (!isValidTouchInput(source, sourceIndex))
                return false;

            if (isCursorTouch(source))
                return base.OnTouchDown(e);

            var touchAction = touchTapActionsDictionary[source];

            osuInputManager.KeyBindingContainer.TriggerPressed(touchAction);

            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;
            int sourceIndex = getTouchIndex(source);

            if (!isValidTouchInput(source, sourceIndex))
                return;

            if (isTapTouch(source))
            {
                var touchAction = touchTapActionsDictionary[source];
                osuInputManager.KeyBindingContainer.TriggerReleased(touchAction);
            }

            base.OnTouchUp(e);
        }
    }
}
