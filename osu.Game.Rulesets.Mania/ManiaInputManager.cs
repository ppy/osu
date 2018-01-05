// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaInputManager : RulesetInputManager<ManiaAction>
    {
        public ManiaInputManager(RulesetInfo ruleset, int variant)
            : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum ManiaAction
    {
        [Description("Special")]
        Special,
        [Description("Key 1")]
        Key1 = 10,
        [Description("Key 2")]
        Key2,
        [Description("Key 3")]
        Key3,
        [Description("Key 4")]
        Key4,
        [Description("Key 5")]
        Key5,
        [Description("Key 6")]
        Key6,
        [Description("Key 7")]
        Key7,
        [Description("Key 8")]
        Key8,
        [Description("Key 9")]
        Key9
    }
}
