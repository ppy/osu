// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : DatabasedKeyBindingInputManager<OsuAction>
    {
        public OsuInputManager(RulesetInfo ruleset) : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
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
