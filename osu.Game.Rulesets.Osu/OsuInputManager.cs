// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : RulesetInputManager<OsuAction>
    {
        public IEnumerable<OsuAction> PressedActions => KeyBindingContainer.PressedActions;

        public bool AllowUserPresses
        {
            set => ((OsuKeyBindingContainer)KeyBindingContainer).AllowUserPresses = value;
        }

        public OsuAction? BlockedButton
        {
            get => ((OsuKeyBindingContainer)KeyBindingContainer).BlockedButton;
            set => ((OsuKeyBindingContainer)KeyBindingContainer).BlockedButton = value;
        }

        public OsuAction? LastButton => ((OsuKeyBindingContainer)KeyBindingContainer).LastButton;

        /// <summary>
        /// Whether the user's cursor movement events should be accepted.
        /// Can be used to block only movement while still accepting button input.
        /// </summary>
        public bool AllowUserCursorMovement { get; set; } = true;

        protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        protected override bool Handle(UIEvent e)
        {
            if (e is MouseMoveEvent && !AllowUserCursorMovement) return false;

            return base.Handle(e);
        }

        private class OsuKeyBindingContainer : RulesetKeyBindingContainer
        {
            public bool AllowUserPresses = true;
            public OsuAction? BlockedButton;
            public OsuAction? LastButton;

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!AllowUserPresses) return false;

                if (e is KeyboardEvent || e is MouseButtonEvent)
                {
                    var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                    var combos = KeyBindings.ToList().Where(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any)).ToList();

                    if (combos.Any())
                    {
                        if (e is KeyDownEvent || e is MouseDownEvent)
                            LastButton = combos.Find(c => c.GetAction<OsuAction>() != BlockedButton)?.GetAction<OsuAction>();
                        else
                            return base.Handle(e);

                        var key = e is KeyboardEvent ? KeyCombination.FromKey(((KeyboardEvent)e).Key) : KeyCombination.FromMouseButton(((MouseButtonEvent)e).Button);
                        var single = combos.FirstOrDefault(c => c.KeyCombination.Keys.Any(k => k == key));
                        if (single?.GetAction<OsuAction>() == BlockedButton)
                            return false;
                    }
                }

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
