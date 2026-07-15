// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation.Taiko;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko
{
    [Cached] // Used for touch input, see DrumTouchInputArea.
    public partial class TaikoInputManager : RulesetInputManager<TaikoAction>
    {
        public TaikoInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum TaikoAction
    {
        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.LeftRim))]
        LeftRim,

        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.LeftCentre))]
        LeftCentre,

        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.RightCentre))]
        RightCentre,

        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.RightRim))]
        RightRim
    }
}
