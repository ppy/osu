// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ProfileRulesetSelector : RulesetSelector
    {
        private Color4 accentColour = Color4.White;

        public readonly Bindable<User> User = new Bindable<User>();

        public ProfileRulesetSelector()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(10, 0);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            accentColour = colours.Seafoam;

            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).AccentColour = accentColour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(u => SetDefaultRuleset(Rulesets.GetRuleset(u.NewValue?.PlayMode ?? "osu")), true);
        }

        public void SetDefaultRuleset(RulesetInfo ruleset)
        {
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).IsDefault = ((ProfileRulesetTabItem)tabItem).Value.ID == ruleset.ID;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ProfileRulesetTabItem(value)
        {
            AccentColour = accentColour
        };

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
        };
    }
}
