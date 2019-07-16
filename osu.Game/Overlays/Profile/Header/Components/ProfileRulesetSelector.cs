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

            User.BindValueChanged(onUserChanged);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            accentColour = colours.Seafoam;

            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).AccentColour = accentColour;
        }

        private void onUserChanged(ValueChangedEvent<User> user)
        {
            SetDefaultRuleset(Rulesets.GetRuleset(user.NewValue.PlayMode ?? "osu"));
            SelectDefaultRuleset();
        }

        public void SetDefaultRuleset(RulesetInfo ruleset)
        {
            // Todo: This method shouldn't exist, but bindables don't provide the concept of observing a change to the default value
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).IsDefault = ((ProfileRulesetTabItem)tabItem).Value.ID == ruleset.ID;
        }

        public void SelectDefaultRuleset()
        {
            // Todo: This method shouldn't exist, but bindables don't provide the concept of observing a change to the default value
            foreach (TabItem<RulesetInfo> tabItem in TabContainer)
            {
                if (((ProfileRulesetTabItem)tabItem).IsDefault)
                {
                    Current.Value = ((ProfileRulesetTabItem)tabItem).Value;
                    return;
                }
            }
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
