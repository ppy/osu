// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Rulesets
{
    public abstract class RulesetSelector : TabControl<RulesetInfo>
    {
        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var r in Rulesets.AvailableRulesets)
                AddItem(r);
        }
    }
}
