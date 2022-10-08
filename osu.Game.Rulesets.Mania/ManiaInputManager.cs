// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania
{
    [Cached] // Used for touch input, see ColumnTouchInputArea.
    public class ManiaInputManager : RulesetInputManager<ManiaAction>
    {
        public ManiaInputManager(RulesetInfo ruleset, int variant)
            : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum ManiaAction
    {
        [Description("特殊键位 1")]
        Special1 = 1,

        [Description("特殊键位 2")]
        Special2,

        // This offsets the start value of normal keys in-case we add more special keys
        // above at a later time, without breaking replays/configs.
        [Description("键位 1")]
        Key1 = 10,

        [Description("键位 2")]
        Key2,

        [Description("键位 3")]
        Key3,

        [Description("键位 4")]
        Key4,

        [Description("键位 5")]
        Key5,

        [Description("键位 6")]
        Key6,

        [Description("键位 7")]
        Key7,

        [Description("键位 8")]
        Key8,

        [Description("键位 9")]
        Key9,

        [Description("键位 10")]
        Key10,

        [Description("键位 11")]
        Key11,

        [Description("键位 12")]
        Key12,

        [Description("键位 13")]
        Key13,

        [Description("键位 14")]
        Key14,

        [Description("键位 15")]
        Key15,

        [Description("键位 16")]
        Key16,

        [Description("键位 17")]
        Key17,

        [Description("键位 18")]
        Key18,
        [Description("键位 19")]
        Key19,

        [Description("键位 20")]
        Key20,
    }
}
