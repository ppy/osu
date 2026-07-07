// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation;
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
        [LocalisableDescription(typeof(RulesetActionStrings), nameof(RulesetActionStrings.CatchMoveLeft))]
        MoveLeft,

        [LocalisableDescription(typeof(RulesetActionStrings), nameof(RulesetActionStrings.CatchMoveRight))]
        MoveRight,

        [LocalisableDescription(typeof(RulesetActionStrings), nameof(RulesetActionStrings.CatchDash))]
        Dash,
    }
}
