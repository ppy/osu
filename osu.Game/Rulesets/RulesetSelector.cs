// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Logging;

namespace osu.Game.Rulesets
{
    public abstract partial class RulesetSelector : TabControl<RulesetInfo>
    {
        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        private readonly bool legacyOnly;

        public RulesetSelector(bool legacyOnly = false)
        {
            this.legacyOnly = legacyOnly;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var ruleset in Rulesets.AvailableRulesets)
            {
                int id = ruleset.OnlineID;

                if ((id < 0 || id > 3) && legacyOnly)
                    continue;

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
