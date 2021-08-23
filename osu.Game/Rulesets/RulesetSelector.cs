// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets
{
    public abstract class RulesetSelector : TabControl<RulesetInfo>
    {
        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        protected virtual bool SelectInitialRuleset => true;

        protected RulesetSelector()
        {
            SelectFirstTabByDefault = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var r in Rulesets.AvailableRulesets)
                AddItem(r);

            if (SelectInitialRuleset)
            {
                // This is supposed to be an implicit process in the base class, but the problem is that it happens in LoadComplete.
                // That can become an issue with overlays that require access to the initial ruleset value
                // before the ruleset selectors reached a LoadComplete state.
                // (e.g. displaying RankingsOverlay for the first time).
                Current.Value = Items.First();
            }
        }
    }
}
