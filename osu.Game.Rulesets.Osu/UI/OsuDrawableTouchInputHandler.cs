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

        /// <summary>
        /// How many taps (taps referring as streaming touch input) can be registered.
        /// </summary>
        private const int tap_touches_limit = 2;

        /// <summary>
        /// The index for the last concurrent able touch.
        /// </summary>
        private const int last_concurrent_touch_index = tap_touches_limit;

        /// <summary>
        /// How many concurrent touches can be registered.
        /// </summary>
        private const int concurrent_touches_limit = last_concurrent_touch_index + 1;

        public readonly HashSet<TouchSource> AllowedTouchSources = Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().Take(concurrent_touches_limit).ToHashSet();

        private readonly Dictionary<TouchSource, OsuAction> touchTapActionsDictionary = new Dictionary<TouchSource, OsuAction>();

        private readonly OsuInputManager osuInputManager;

        private int getTouchIndex(TouchSource source)
        {
            return source - TouchSource.Touch1;
        }

        public OsuDrawableTouchInputHandler(OsuInputManager inputManager)
        {
            osuInputManager = inputManager;
            foreach (var source in AllowedTouchSources)
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

        private bool isValidTouchInput(int index)
        {
            return index <= last_concurrent_touch_index;
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            var source = e.Touch.Source;
            int sourceIndex = getTouchIndex(source);

            osuInputManager.DragMode = sourceIndex >= last_concurrent_touch_index;

            // A cursor touch is always a valid touch.
            if (isCursorTouch(source))
                return base.OnTouchDown(e);

            if (!isValidTouchInput(sourceIndex))
                return false;

            var touchAction = touchTapActionsDictionary[source];

            osuInputManager.KeyBindingContainer.TriggerPressed(touchAction);

            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            var source = e.Touch.Source;
            int sourceIndex = getTouchIndex(source);

            if (!isValidTouchInput(sourceIndex))
                return;

            if (isTapTouch(source))
                osuInputManager.KeyBindingContainer.TriggerReleased(touchTapActionsDictionary[source]);

            base.OnTouchUp(e);
        }
    }
}
