// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ProfileRulesetSelector : RulesetSelector
    {
        private Color4 accentColour = Color4.White;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                {
                    ((RulesetTabItem)tabItem).AccentColour = value;
                }
            }
        }

        public ProfileRulesetSelector()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(10, 0);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Seafoam;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new RulesetTabItem(value)
        {
            AccentColour = AccentColour
        };

        public void SetDefaultRuleset(RulesetInfo ruleset)
        {
            // Todo: This method shouldn't exist, but bindables don't provide the concept of observing a change to the default value
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                ((RulesetTabItem)tabItem).IsDefault = ((RulesetTabItem)tabItem).Value.ID == ruleset.ID;
        }

        public void SelectDefaultRuleset()
        {
            // Todo: This method shouldn't exist, but bindables don't provide the concept of observing a change to the default value
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
            {
                if (((RulesetTabItem)tabItem).IsDefault)
                {
                    Current.Value = ((RulesetTabItem)tabItem).Value;
                    return;
                }
            }
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
        };
    }
}
