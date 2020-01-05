// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        [Description("特殊键位1")]
        Special1 = 1,

        [Description("特殊键位2")]
        Special2,

        // This offsets the start value of normal keys in-case we add more special keys
        // above at a later time, without breaking replays/configs.
        [Description("键位1")]
        Key1 = 10,

        [Description("键位2")]
        Key2,

        [Description("键位3")]
        Key3,

        [Description("键位4")]
        Key4,

        [Description("键位5")]
        Key5,

        [Description("键位6")]
        Key6,

        [Description("键位7")]
        Key7,

        [Description("键位8")]
        Key8,

        [Description("键位9")]
        Key9,

        [Description("键位10")]
        Key10,

        [Description("键位11")]
        Key11,

        [Description("键位12")]
        Key12,

        [Description("键位13")]
        Key13,

        [Description("键位14")]
        Key14,

        [Description("键位15")]
        Key15,

        [Description("键位16")]
        Key16,

        [Description("键位17")]
        Key17,

        [Description("键位18")]
        Key18,
    }
}
