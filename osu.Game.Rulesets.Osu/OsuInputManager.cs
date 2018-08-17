// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
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

        protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        private class OsuKeyBindingContainer : RulesetKeyBindingContainer
        {
            public bool AllowUserPresses = true;

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args) => AllowUserPresses && base.OnKeyDown(state, args);
            protected override bool OnKeyUp(InputState state, KeyUpEventArgs args) => AllowUserPresses && base.OnKeyUp(state, args);
            protected override bool OnJoystickPress(InputState state, JoystickEventArgs args) => AllowUserPresses && base.OnJoystickPress(state, args);
            protected override bool OnJoystickRelease(InputState state, JoystickEventArgs args) => AllowUserPresses && base.OnJoystickRelease(state, args);
            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => AllowUserPresses && base.OnMouseDown(state, args);
            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => AllowUserPresses && base.OnMouseUp(state, args);
            protected override bool OnScroll(InputState state) => AllowUserPresses && base.OnScroll(state);
        }
    }

    public enum OsuAction
    {
        [Description("Left Button")]
        LeftButton,

        [Description("Right Button")]
        RightButton
    }
}
