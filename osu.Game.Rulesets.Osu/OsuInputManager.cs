// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using OpenTK.Input;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : DatabasedKeyBindingInputManager<OsuAction>
    {
        public OsuInputManager(RulesetInfo ruleset) : base(ruleset, simultaneousMode: SimultaneousBindingMode.Unique)
        {

        }
        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            var mouse = state.Mouse as MouseState;
            var keyboard = state.Keyboard as KeyboardState;

            if (mouse != null && keyboard != null)
            {
                if (mouse.IsPressed(MouseButton.Left))
                    keyboard.Keys = keyboard.Keys.Concat(new[] { Key.LastKey + 1 });
                if (mouse.IsPressed(MouseButton.Right))
                    keyboard.Keys = keyboard.Keys.Concat(new[] { Key.LastKey + 2 });
            }
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
