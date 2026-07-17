// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation.Catch;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch
{
    [Cached]
    public partial class CatchInputManager : RulesetInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum CatchAction
    {
        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.MoveLeft))]
        MoveLeft,

        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.MoveRight))]
        MoveRight,

        [LocalisableDescription(typeof(ActionStrings), nameof(ActionStrings.Dash))]
        Dash,
    }
}
