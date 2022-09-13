// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : RulesetInputManager<OsuAction>
    {
        public IEnumerable<OsuAction> PressedActions => KeyBindingContainer.PressedActions;

        private readonly OsuTouchInputMapper touchInputMapper;

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
            touchInputMapper = new OsuTouchInputMapper(this) { RelativeSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load() => Add(touchInputMapper);

        protected override bool Handle(UIEvent e)
        {
            if ((e is MouseMoveEvent || e is TouchMoveEvent) && !AllowUserCursorMovement) return false;

            return base.Handle(e);
        }

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e)
        {
            var source = e.Touch.Source;

            if (touchInputMapper.IsTapTouch(source))
                return true;

            return base.HandleMouseTouchStateChange(e);
        }

        /// <summary>
        /// Disables mouse input for the first touch, so <see cref="OsuTouchInputMapper"/> can handle input for both keys for streaming.
        /// </summary>
        /// <returns>Whether we entered an touch tap only mapping state</returns>
        public bool HandleTouchTapOnlyMapping()
        {
            Vector2? cursorTouchPosition;

            if (!touchInputMapper.IsCursorTouch(OsuTouchInputMapper.DEFAULT_CURSOR_TOUCH) || ((cursorTouchPosition = CurrentState.Touch.GetTouchPosition(OsuTouchInputMapper.DEFAULT_CURSOR_TOUCH)) == null))
                return false;

            base.HandleMouseTouchStateChange(new TouchStateChangeEvent(CurrentState, null, new Touch(OsuTouchInputMapper.DEFAULT_CURSOR_TOUCH, cursorTouchPosition.Value), false, cursorTouchPosition));

            return true;
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
