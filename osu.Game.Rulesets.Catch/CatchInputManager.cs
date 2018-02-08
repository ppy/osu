// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        [Description("Move left")]
        MoveLeft,
        [Description("Move right")]
        MoveRight,
        [Description("Engage dash")]
        Dash,
    }
}
