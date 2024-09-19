// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania
{
    [Cached] // Used for touch input, see ColumnTouchInputArea.
    public partial class ManiaInputManager : RulesetInputManager<ManiaAction>
    {
        public ManiaInputManager(RulesetInfo ruleset, int variant)
            : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum ManiaAction
    {
        [Description("Key 1")]
        Key1,

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
        Key9,

        [Description("Key 10")]
        Key10,

        [Description("Key 11")]
        Key11,

        [Description("Key 12")]
        Key12,

        [Description("Key 13")]
        Key13,

        [Description("Key 14")]
        Key14,

        [Description("Key 15")]
        Key15,

        [Description("Key 16")]
        Key16,

        [Description("Key 17")]
        Key17,

        [Description("Key 18")]
        Key18,

        [Description("Key 19")]
        Key19,

        [Description("Key 20")]
        Key20,
    }
}
