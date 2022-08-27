// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : RulesetInputManager<OsuAction>
    {
        private const TouchSource cursor_touch = TouchSource.Touch1;

        private const int tap_touches_limit = 2;
        private const int simultaneous_touches_limit = tap_touches_limit + 1;

        private readonly HashSet<TouchSource> activeTapTouches = new HashSet<TouchSource>();

        private readonly Dictionary<TouchSource, OsuAction> touchTapActionsDictionary = Enum.GetValues(typeof(TouchSource))
                                                                                            .Cast<TouchSource>()
                                                                                            .Take(simultaneous_touches_limit)
                                                                                            .ToDictionary(source => source, source => (source - TouchSource.Touch1) % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton);

        public IEnumerable<OsuAction> PressedActions => KeyBindingContainer.PressedActions;

        public bool AllowUserPresses
        {
            set => ((OsuKeyBindingContainer)KeyBindingContainer).AllowUserPresses = value;
        }

        /// <summary>
        /// Whether the user's cursor movement events should be accepted.
        /// Can be used to block only movement while still accepting button input.
        /// </summary>
        public bool AllowUserCursorMovement { get; set; } = true;

        protected override KeyBindingContainer<OsuAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        protected override bool Handle(UIEvent e)
        {
            if ((e is MouseMoveEvent || e is TouchMoveEvent) && !AllowUserCursorMovement) return false;

            return base.Handle(e);
        }

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e)
        {
            var source = e.Touch.Source;

            int touchNumber = source - TouchSource.Touch1 + 1;

            if (touchNumber > simultaneous_touches_limit)
                return false;

            bool isTapTouch = source != cursor_touch || !AllowUserCursorMovement;
            bool isCursorTouch = !isTapTouch;

            if (isTapTouch && !activeTapTouches.Contains(source))
            {
                activeTapTouches.Add(source);
                KeyBindingContainer.TriggerPressed(touchTapActionsDictionary[source]);
            }

            var disabledTapTouches = activeTapTouches.Where(tap => !CurrentState.Touch.IsActive(tap));

            if (disabledTapTouches.Any())
                foreach (var tap in disabledTapTouches.ToList())
                {
                    activeTapTouches.Remove(tap);
                    KeyBindingContainer.TriggerReleased(touchTapActionsDictionary[tap]);
                }

            // HashSet count implementation is o(1) so this is fine.
            bool doubletapping = activeTapTouches.Count() == tap_touches_limit;

            if (doubletapping)
            {
                e = new TouchStateChangeEvent(e.State, e.Input, e.Touch, false, e.LastPosition);
            }

            return AllowUserCursorMovement && isCursorTouch && base.HandleMouseTouchStateChange(e);
        }

        private class OsuKeyBindingContainer : RulesetKeyBindingContainer
        {
            public bool AllowUserPresses = true;

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!AllowUserPresses) return false;

                return base.Handle(e);
            }
        }
    }

    public enum OsuAction
    {
        [Description("Left button")]
        LeftButton,

        [Description("Right button")]
        RightButton
    }
}
