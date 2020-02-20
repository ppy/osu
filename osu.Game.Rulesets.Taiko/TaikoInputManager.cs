// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoInputManager : RulesetInputManager<TaikoAction>
    {
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
