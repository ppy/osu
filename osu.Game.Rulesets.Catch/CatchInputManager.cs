// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch
{
    public class CatchInputManager : RulesetInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum CatchAction
    {
        [Description("向左移动")]
        MoveLeft,

        [Description("向右移动")]
        MoveRight,

        [Description("冲刺")]
        Dash,
    }
}
