// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class GamemodeControl : TabControl<RulesetInfo>
    {
        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new GamemodeTabItem(value)
        {
            AccentColour = AccentColour
        };

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
                    ((GamemodeTabItem)tabItem).AccentColour = value;
                }
            }
        }

        public GamemodeControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(10, 0);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (var r in rulesets.AvailableRulesets)
            {
                AddItem(r);
            }
        }

        public void SetDefaultGamemode(string gamemode)
        {
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
            {
                ((GamemodeTabItem)tabItem).IsDefault = ((GamemodeTabItem)tabItem).Value.ShortName == gamemode;
            }
        }

        public void SelectDefaultGamemode()
        {
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
            {
                if (((GamemodeTabItem)tabItem).IsDefault)
                {
                    Current.Value = ((GamemodeTabItem)tabItem).Value;
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
