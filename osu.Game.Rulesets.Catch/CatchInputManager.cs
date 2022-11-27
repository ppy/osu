// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch
{
    [Cached]
    public partial class CatchInputManager : RulesetInputManager<CatchAction>
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
