// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Team.Header.Components
{
    public partial class TeamRulesetSelector : OverlayRulesetSelector
    {
        [Resolved]
        private TeamProfileOverlay? profileOverlay { get; set; }

        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TeamData.BindValueChanged(data => updateState(data.NewValue));
            Current.BindValueChanged(ruleset =>
            {
                if (TeamData.Value != null && !ruleset.NewValue.Equals(TeamData.Value.Ruleset))
                    profileOverlay?.ShowTeam(TeamData.Value.Team, ruleset.NewValue);
            });
        }

        private void updateState(TeamProfileData? data)
        {
            Current.Value = Items.SingleOrDefault(ruleset => data?.Ruleset.MatchesOnlineID(ruleset) == true);
            setDefaultRuleset(Rulesets.GetRuleset(data?.Team.DefaultRulesetId ?? 0).AsNonNull());
        }

        private void setDefaultRuleset(RulesetInfo ruleset)
        {
            foreach (var tabItem in TabContainer)
                ((ProfileRulesetTabItem)tabItem).IsDefault = ((ProfileRulesetTabItem)tabItem).Value.Equals(ruleset);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ProfileRulesetTabItem(value);
    }
}
