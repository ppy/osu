// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania
{
    [Cached] // Used for touch input, see Column.OnTouchDown/OnTouchUp.
    public partial class ManiaInputManager : RulesetInputManager<ManiaAction>
    {
        public ManiaInputManager(RulesetInfo ruleset, int variant)
            : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum ManiaAction
    {
        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key1))]
        Key1,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key2))]
        Key2,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key3))]
        Key3,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key4))]
        Key4,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key5))]
        Key5,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key6))]
        Key6,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key7))]
        Key7,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key8))]
        Key8,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key9))]
        Key9,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key10))]
        Key10,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key11))]
        Key11,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key12))]
        Key12,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key13))]
        Key13,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key14))]
        Key14,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key15))]
        Key15,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key16))]
        Key16,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key17))]
        Key17,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key18))]
        Key18,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key19))]
        Key19,

        [LocalisableDescription(typeof(ManiaActionStrings), nameof(ManiaActionStrings.Key20))]
        Key20,
    }

    file class ManiaActionStrings
    {
        public static LocalisableString Key1 => RulesetActionStrings.ManiaKey(1);
        public static LocalisableString Key2 => RulesetActionStrings.ManiaKey(2);
        public static LocalisableString Key3 => RulesetActionStrings.ManiaKey(3);
        public static LocalisableString Key4 => RulesetActionStrings.ManiaKey(4);
        public static LocalisableString Key5 => RulesetActionStrings.ManiaKey(5);
        public static LocalisableString Key6 => RulesetActionStrings.ManiaKey(6);
        public static LocalisableString Key7 => RulesetActionStrings.ManiaKey(7);
        public static LocalisableString Key8 => RulesetActionStrings.ManiaKey(8);
        public static LocalisableString Key9 => RulesetActionStrings.ManiaKey(9);
        public static LocalisableString Key10 => RulesetActionStrings.ManiaKey(10);
        public static LocalisableString Key11 => RulesetActionStrings.ManiaKey(11);
        public static LocalisableString Key12 => RulesetActionStrings.ManiaKey(12);
        public static LocalisableString Key13 => RulesetActionStrings.ManiaKey(13);
        public static LocalisableString Key14 => RulesetActionStrings.ManiaKey(14);
        public static LocalisableString Key15 => RulesetActionStrings.ManiaKey(15);
        public static LocalisableString Key16 => RulesetActionStrings.ManiaKey(16);
        public static LocalisableString Key17 => RulesetActionStrings.ManiaKey(17);
        public static LocalisableString Key18 => RulesetActionStrings.ManiaKey(18);
        public static LocalisableString Key19 => RulesetActionStrings.ManiaKey(19);
        public static LocalisableString Key20 => RulesetActionStrings.ManiaKey(20);
    }
}
