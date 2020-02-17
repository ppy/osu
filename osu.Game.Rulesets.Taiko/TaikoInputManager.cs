// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoInputManager : RulesetInputManager<TaikoAction>
    {
        protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new TaikoKeyBindingContainer(ruleset, variant, unique);

        public event Func<UIEvent, IEnumerable<KeyBinding>, bool> BlockConditions
        {
            add => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockConditions += value;
            remove => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockConditions -= value;
        }

        public TaikoInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        public void PressKey(Key key, bool pressed)
        {
            var input = new KeyboardKeyInput(key, true);

            var evt = new ButtonStateChangeEvent<Key>(CurrentState, input, key, pressed ? ButtonStateChangeKind.Pressed : ButtonStateChangeKind.Released);
            HandleInputStateChange(evt);
        }

        public class TaikoKeyBindingContainer : RulesetKeyBindingContainer
        {
            public event Func<UIEvent, IEnumerable<KeyBinding>, bool> BlockConditions;

            public TaikoKeyBindingContainer(RulesetInfo info, int variant, SimultaneousBindingMode unique)
                : base(info, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (BlockConditions?.Invoke(e, KeyBindings) == true)
                    return false;

                return base.Handle(e);
            }
        }
    }

    public enum TaikoAction
    {
        [Description("Left (rim)")]
        LeftRim,

        [Description("Left (centre)")]
        LeftCentre,

        [Description("Right (centre)")]
        RightCentre,

        [Description("Right (rim)")]
        RightRim
    }
}
