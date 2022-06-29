// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Logging;

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
            foreach (var ruleset in Rulesets.AvailableRulesets)
            {
                try
                {
                    AddItem(ruleset);
                }
                catch
                {
                    Logger.Log($"Could not create ruleset icon for {ruleset.Name}. Please check for an update from the developer.", level: LogLevel.Error);
                }
            }
        }
    }
}
