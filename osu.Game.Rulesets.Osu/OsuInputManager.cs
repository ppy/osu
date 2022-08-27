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
        private const int tap_touches_limit = 2;
        private const int simultaneous_touches_limit = tap_touches_limit + 1;

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

        private readonly Dictionary<TouchSource, int> numberedTouchSources = Enum.GetValues(typeof(TouchSource))
                                                                                 .Cast<TouchSource>()
                                                                                 .Select((v, i) => new { v, i })
                                                                                 .ToDictionary(entry => entry.v, entry => entry.i + 1);

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

        private OsuAction getActionForTouchSource(TouchSource source)
        {
            int sourceNumber = numberedTouchSources[source];
            return sourceNumber % 2 == 0 ? OsuAction.RightButton : OsuAction.LeftButton;
        }

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e)
        {
            var source = e.Touch.Source;

            int touchNumber = numberedTouchSources[source];

            if (touchNumber > simultaneous_touches_limit)
                return false;

            var activeSources = CurrentState.Touch.ActiveSources;

            var cursorTouch = activeSources.Any() ? activeSources.First() : source;

            var activeTapTouches = AllowUserCursorMovement ? activeSources.Skip(1) : activeSources;
            var limitedActiveTapTouches = activeTapTouches.Take(tap_touches_limit);

            var newActiveTapActions = limitedActiveTapTouches.Select(getActionForTouchSource);
            var newInactiveTapActions = PressedActions.Where(a => !newActiveTapActions.Contains(a)).ToList();

            bool isCursorTouch = source == cursorTouch;
            bool isTapTouch = !isCursorTouch;

            foreach (var action in newInactiveTapActions)
                KeyBindingContainer.TriggerReleased(action);

            if (isTapTouch)
            {
                var action = getActionForTouchSource(source);
                if (!PressedActions.Contains(action))
                    KeyBindingContainer.TriggerPressed(action);
            }

            bool doubletapping = limitedActiveTapTouches.Count() == tap_touches_limit;

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
