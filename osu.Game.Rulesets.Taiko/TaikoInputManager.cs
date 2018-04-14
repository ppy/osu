// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoInputManager : RulesetInputManager<TaikoAction>
    {
        public TaikoInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum TaikoAction
    {
        [Description("Left (Rim)")]
        LeftRim,
        [Description("Left (Centre)")]
        LeftCentre,
        [Description("Right (Centre)")]
        RightCentre,
        [Description("Right (Rim)")]
        RightRim
    }
}
