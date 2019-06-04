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

                foreach (GamemodeTabItem tabItem in TabContainer)
                {
                    tabItem.AccentColour = value;
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
            foreach (GamemodeTabItem i in TabContainer)
            {
                i.IsDefault = i.Value.ShortName == gamemode;
            }
        }

        public void SelectDefaultGamemode()
        {
            foreach (GamemodeTabItem i in TabContainer)
            {
                if (i.IsDefault)
                {
                    Current.Value = i.Value;
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
