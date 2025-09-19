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
        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey1))]
        Key1,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey2))]
        Key2,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey3))]
        Key3,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey4))]
        Key4,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey5))]
        Key5,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey6))]
        Key6,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey7))]
        Key7,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey8))]
        Key8,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey9))]
        Key9,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey10))]
        Key10,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey11))]
        Key11,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey12))]
        Key12,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey13))]
        Key13,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey14))]
        Key14,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey15))]
        Key15,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey16))]
        Key16,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey17))]
        Key17,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey18))]
        Key18,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey19))]
        Key19,

        [LocalisableDescription(typeof(RulesetActionsStrings), nameof(RulesetActionsStrings.ManiaKey20))]
        Key20,
    }
}
