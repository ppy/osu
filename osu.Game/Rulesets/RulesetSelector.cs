// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets
{
    public abstract class RulesetSelector : TabControl<RulesetInfo>
    {
        protected RulesetStore AvaliableRulesets;

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            AvaliableRulesets = rulesets;

            foreach (var r in rulesets.AvailableRulesets)
            {
                AddItem(r);
            }
        }
    }
}
