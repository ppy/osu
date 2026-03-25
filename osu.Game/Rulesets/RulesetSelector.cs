// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;

namespace osu.Game.Rulesets
{
    public abstract partial class RulesetSelector : TabControl<RulesetInfo>
    {
        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        protected virtual bool LegacyOnly => false;

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var ruleset in Rulesets.AvailableRulesets)
            {
                if (!ruleset.IsLegacyRuleset() && LegacyOnly)
                    continue;

                try
                {
                    AddItem(ruleset);
                }
                catch (Exception e)
                {
                    RulesetStore.LogRulesetFailure(ruleset, e);
                }
            }
        }
    }
}
