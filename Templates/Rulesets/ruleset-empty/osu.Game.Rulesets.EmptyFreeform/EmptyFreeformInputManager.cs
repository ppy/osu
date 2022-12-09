// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.EmptyFreeform
{
    public partial class EmptyFreeformInputManager : RulesetInputManager<EmptyFreeformAction>
    {
        public EmptyFreeformInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum EmptyFreeformAction
    {
        [Description("Button 1")]
        Button1,

        [Description("Button 2")]
        Button2,
    }
}
