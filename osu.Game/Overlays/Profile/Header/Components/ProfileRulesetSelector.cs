// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ProfileRulesetSelector : OverlayRulesetSelector
    {
        [Resolved]
        private UserProfileOverlay? profileOverlay { get; set; }

        public readonly Bindable<UserProfile?> UserProfile = new Bindable<UserProfile?>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UserProfile.BindValueChanged(userProfile => updateState(userProfile.NewValue), true);
            Current.BindValueChanged(ruleset =>
            {
                if (UserProfile.Value != null && !ruleset.NewValue.Equals(UserProfile.Value.Ruleset))
                    profileOverlay?.ShowUser(UserProfile.Value.User, ruleset.NewValue);
            });
        }

        private void updateState(UserProfile? userProfile)
        {
            Current.Value = Items.SingleOrDefault(ruleset => userProfile?.Ruleset.MatchesOnlineID(ruleset) == true);
            SetDefaultRuleset(Rulesets.GetRuleset(userProfile?.User.PlayMode ?? @"osu").AsNonNull());
        }

        public void SetDefaultRuleset(RulesetInfo ruleset)
        {
            foreach (var tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).IsDefault = ((ProfileRulesetTabItem)tabItem).Value.Equals(ruleset);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ProfileRulesetTabItem(value);
    }
}
