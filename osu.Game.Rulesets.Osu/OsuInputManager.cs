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

        public int BlockedKeystrokes => ((OsuKeyBindingContainer)KeyBindingContainer).BlockedKeystrokes;

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
            public int BlockedKeystrokes;
            public OsuAction? LastButton;

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!AllowUserPresses) return false;

                var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                var combos = KeyBindings?.ToList().FindAll(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any));

                InputKey? key;
                var kb = e as KeyDownEvent;
                var mouse = e as MouseDownEvent;
                if (kb != null)
                    key = KeyCombination.FromKey(kb.Key);
                else if (mouse != null)
                    key = KeyCombination.FromMouseButton(mouse.Button);
                else
                    return base.Handle(e);

                var single = combos?.Find(c => c.KeyCombination.Keys.Any(k => k == key))?.GetAction<OsuAction>();

                LastButton = single;

                if (single != null && single == BlockedButton)
                {
                    if (kb != null && !kb.Repeat)
                        BlockedKeystrokes++;
                    else if (mouse != null)
                        BlockedKeystrokes++;
                    return false;
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
