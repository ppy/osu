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
        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key1))]
        Key1,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key2))]
        Key2,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key3))]
        Key3,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key4))]
        Key4,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key5))]
        Key5,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key6))]
        Key6,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key7))]
        Key7,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key8))]
        Key8,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key9))]
        Key9,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key10))]
        Key10,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key11))]
        Key11,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key12))]
        Key12,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key13))]
        Key13,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key14))]
        Key14,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key15))]
        Key15,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key16))]
        Key16,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key17))]
        Key17,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key18))]
        Key18,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key19))]
        Key19,

        [LocalisableDescription(typeof(ManiaActionStringsHelper), nameof(ManiaActionStringsHelper.Key20))]
        Key20,
    }

    static file class ManiaActionStringsHelper
    {
        public static LocalisableString Key1 => ManiaActionStrings.Key(1);
        public static LocalisableString Key2 => ManiaActionStrings.Key(2);
        public static LocalisableString Key3 => ManiaActionStrings.Key(3);
        public static LocalisableString Key4 => ManiaActionStrings.Key(4);
        public static LocalisableString Key5 => ManiaActionStrings.Key(5);
        public static LocalisableString Key6 => ManiaActionStrings.Key(6);
        public static LocalisableString Key7 => ManiaActionStrings.Key(7);
        public static LocalisableString Key8 => ManiaActionStrings.Key(8);
        public static LocalisableString Key9 => ManiaActionStrings.Key(9);
        public static LocalisableString Key10 => ManiaActionStrings.Key(10);
        public static LocalisableString Key11 => ManiaActionStrings.Key(11);
        public static LocalisableString Key12 => ManiaActionStrings.Key(12);
        public static LocalisableString Key13 => ManiaActionStrings.Key(13);
        public static LocalisableString Key14 => ManiaActionStrings.Key(14);
        public static LocalisableString Key15 => ManiaActionStrings.Key(15);
        public static LocalisableString Key16 => ManiaActionStrings.Key(16);
        public static LocalisableString Key17 => ManiaActionStrings.Key(17);
        public static LocalisableString Key18 => ManiaActionStrings.Key(18);
        public static LocalisableString Key19 => ManiaActionStrings.Key(19);
        public static LocalisableString Key20 => ManiaActionStrings.Key(20);
    }
}
