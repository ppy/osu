// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ProfileRulesetSelector : OverlayRulesetSelector
    {
        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(u => SetDefaultRuleset(Rulesets.GetRuleset(u.NewValue?.PlayMode ?? "osu")), true);
        }

        public void SetDefaultRuleset(RulesetInfo ruleset)
        {
            foreach (var tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).IsDefault = ((ProfileRulesetTabItem)tabItem).Value.ID == ruleset.ID;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ProfileRulesetTabItem(value);
    }
}
