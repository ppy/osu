// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class OverlayRulesetSelector : OsuTabControl<RulesetInfo>
    {
        [Resolved]
        protected RulesetStore Rulesets { get; private set; } = null!;

        public OverlayRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
            TabContainer.Spacing = new Vector2(20, 0);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var ruleset in Rulesets.AvailableRulesets)
            {
                // web does not support legacy rulesets.
                if (!ruleset.IsLegacyRuleset())
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

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new OverlayRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
        };

        protected override Dropdown<RulesetInfo>? CreateDropdown() => null;
    }
}
