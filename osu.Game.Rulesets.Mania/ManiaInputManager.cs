// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation.Mania;
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
        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key1))]
        Key1,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key2))]
        Key2,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key3))]
        Key3,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key4))]
        Key4,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key5))]
        Key5,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key6))]
        Key6,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key7))]
        Key7,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key8))]
        Key8,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key9))]
        Key9,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key10))]
        Key10,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key11))]
        Key11,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key12))]
        Key12,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key13))]
        Key13,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key14))]
        Key14,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key15))]
        Key15,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key16))]
        Key16,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key17))]
        Key17,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key18))]
        Key18,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key19))]
        Key19,

        [LocalisableDescription(typeof(ActionStringsHelper), nameof(ActionStringsHelper.Key20))]
        Key20,
    }

    // Workaround for the inability to pass arguments to `LocalisableDescription`.
    // Should be removed if such a feature is added at all.
    static file class ActionStringsHelper
    {
        public static LocalisableString Key1 => ActionStrings.Key(1);
        public static LocalisableString Key2 => ActionStrings.Key(2);
        public static LocalisableString Key3 => ActionStrings.Key(3);
        public static LocalisableString Key4 => ActionStrings.Key(4);
        public static LocalisableString Key5 => ActionStrings.Key(5);
        public static LocalisableString Key6 => ActionStrings.Key(6);
        public static LocalisableString Key7 => ActionStrings.Key(7);
        public static LocalisableString Key8 => ActionStrings.Key(8);
        public static LocalisableString Key9 => ActionStrings.Key(9);
        public static LocalisableString Key10 => ActionStrings.Key(10);
        public static LocalisableString Key11 => ActionStrings.Key(11);
        public static LocalisableString Key12 => ActionStrings.Key(12);
        public static LocalisableString Key13 => ActionStrings.Key(13);
        public static LocalisableString Key14 => ActionStrings.Key(14);
        public static LocalisableString Key15 => ActionStrings.Key(15);
        public static LocalisableString Key16 => ActionStrings.Key(16);
        public static LocalisableString Key17 => ActionStrings.Key(17);
        public static LocalisableString Key18 => ActionStrings.Key(18);
        public static LocalisableString Key19 => ActionStrings.Key(19);
        public static LocalisableString Key20 => ActionStrings.Key(20);
    }
}
